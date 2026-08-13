/*
 * FireflyController.cpp
 *
 *  Created on: Feb 7, 2016
 *      Author: Denis
 */

#include <animation/AnimationSetPlayer.h>
#include <animation/sets/AnimationSet.h>
#include <FireflyController.h>
#include <glm/glm.hpp>

#if defined _WIN32 || defined _WIN64
#include <GL/gl.h>
#else
#include <OpenGL/gl.h>
#endif

#define CAM_SPEED_HORIZ 0.5
#define CAM_SPEED_VERT 0.3

map<GLFWwindow*, FireflyController*> FireflyController::windowControllers;

FireflyController::FireflyController(const char* title, int width, int height,
		const char* serialPort, PixelStageOption stageType) {
	window = initGL(title, width, height);
	windowControllers[window] = this;

	stage = new PixelStage(stageType);

	cam = new Camera();
	cam->moveTo(stage->getCentroid());

//	activeAnim = new SpheresAnimation();
//	activeAnim = new WorldOfBalloonsAnimation();
	activeAnim = new AnimationSetPlayer(
			stage,
			new AllAnimations(stage),
			new AllPixelShaders(),
			new AllColorPalettes(),
			new AllColorSchemes(),
			10.0, 75.0);

	lastX = -1.0;
	lastY = -1.0;
	moveCamera = false;
	cameraAutoSpin = false;

	serial = new Serial();
	serial->initComms();

	calibration = NULL;
	log("[FFC] Finish controller instantiation");
}

FireflyController::~FireflyController() {
	glfwDestroyWindow(window);
	glfwTerminate();
	cout << "[FFC] GLFW terminated" << endl;
}

void FireflyController::start() {
	double time = glfwGetTime();
	activeAnim->init(time);

	while (!glfwWindowShouldClose(window))
	{
		render(window, serial, glfwGetTime() - time);

		glfwSwapBuffers(window);
		glfwPollEvents();
	}
}

void FireflyController::render(GLFWwindow* window, Serial* serial, double time)
{
	if(smoothingFrames < FFC_MAX_SMOOTHING && (smoothingFrames+1.0)/smoothingFrames*lastProcessTime/(lastProcessTime+lastOutputTime) < 0.8) {
		smoothingFrames++;
		printf("[FFC] Increase smoothing to %d\n", smoothingFrames);
	} else if(smoothingFrames > FFC_MIN_SMOOTHING && lastProcessTime/(lastProcessTime+lastOutputTime) > 0.85) {
		smoothingFrames--;
		printf("[FFC] Decrease smoothing to %d\n", smoothingFrames);
	}

	if (time > nextUpdateTime) {
		std::printf("[FFC] %.2f sec | %.1f FPS | %.2fms process | %.2fms output\n"
				"[FFC] %.2f%% brightness | %.2f mA avg | %d smoothing\n",
				nextUpdateTime,
				frameCount/5.0,
				processTime*1000.0/frameCount,
				outputTime*1000.0/frameCount,
				stage->getBrightness()*100.0,
				cumulativePowerDraw/frameCount,
				smoothingFrames);
		nextUpdateTime += 5.0;
		processTime = 0.0;
		outputTime = 0.0;
		frameCount = 0;
		cumulativePowerDraw = 0.0;
	}

	timeAnchor = glfwGetTime();

	int width, height;
	glfwGetFramebufferSize(window, &width, &height);

	glViewport(0, 0, width, height);
	glClearColor(0.0f, 0.0f, 0.0f, 1.0f);
	glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

	glMatrixMode(GL_PROJECTION);
	glLoadIdentity();
	gluPerspective(60.0, (double)width/(double)height, 0.2, 100.0);

    glMatrixMode(GL_MODELVIEW);
	glLoadIdentity();

	if (cameraAutoSpin)
		cam->rotate(0.5, 0.0);
	if (calibration) {
		glm::vec3 camPos = cam->getPos();
		camPos.z = calibration->pixelInFocus().getZ();
		cam->moveTo(camPos);
	}
	cam->setPerspective();

	clearStage();

	if(calibration) {
		calibration->lightPixels(time);
	}
	else if(smoothingFrames == 1) {
		activeAnim->render(time);
	}
	else {
		double smoothingInterval = (time - lastFrame)/(double)smoothingFrames;
		glm::vec3 *smoothedColors = new glm::vec3[stage->pixelsLen];

		for(int f = 0; f < smoothingFrames; f++) {
			lastFrame += smoothingInterval;
			activeAnim->render(lastFrame);

			for(int p = 0; p < stage->pixelsLen; p++) {
				if(f == 0)
					smoothedColors[p] = stage->pixels[p].getColor();
				else
					smoothedColors[p] += stage->pixels[p].getColor();
			}
		}
		for(int p = 0; p < stage->pixelsLen; p++) {
			stage->pixels[p].setColor(smoothedColors[p]/(float)smoothingFrames);
		}
	}
	lastFrame = time;

	stage->renderGL();
	lastProcessTime = glfwGetTime() - timeAnchor;
	processTime += lastProcessTime;
	timeAnchor = glfwGetTime();

	if(serial->available())
		stage->renderLED(serial);

	lastOutputTime = glfwGetTime() - timeAnchor;
	outputTime += lastOutputTime;
	frameCount++;
	cumulativePowerDraw += stage->getPowerDraw();
}

void FireflyController::clearStage() {
	for(int p = 0; p < stage->pixelsLen; p++) {
		stage->pixels[p].setColor(glm::vec3(0.0, 0.0, 0.0));
	}
}

GLFWwindow* FireflyController::initGL(const char* title, int width, int height)
{
	if (!glfwInit())
	{
		log("[FFC] Error initializing GLFW, exiting");
	    exit(EXIT_FAILURE);
	}

	GLFWwindow* window = glfwCreateWindow(width, height, title, NULL, NULL);
	if (!window)
	{
		log("[FFC] Error creating GLFW window, exiting");
		exit(EXIT_FAILURE);
	}
	glfwMakeContextCurrent(window);

	glfwSetErrorCallback(this->error_callback);
	glfwSetKeyCallback(window, this->route_key_callback);
	glfwSetCursorPosCallback(window, this->route_mouse_move_callback);
	glfwSetMouseButtonCallback(window, this->route_mouse_button_callback);
	glfwSetScrollCallback(window, this->route_mouse_scroll_callback);
	log("[FFC] Callbacks set");

	glClearDepth(1.0f);
	glDepthFunc(GL_LESS);
	glEnable(GL_DEPTH_TEST);
	glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
	glEnable(GL_BLEND);

	return window;
}

void FireflyController::keyPress(int key, int scancode, int action, int mods)
{
	if (key == GLFW_KEY_ESCAPE && action == GLFW_RELEASE)
	{
		glfwSetWindowShouldClose(this->window, GL_TRUE);
	}
	if (key == GLFW_KEY_EQUAL && action != GLFW_RELEASE)
	{
		stage->brightnessUp();
	}
	if (key == GLFW_KEY_MINUS && action != GLFW_RELEASE)
	{
		stage->brightnessDown();
	}
	if (key == GLFW_KEY_SPACE && action == GLFW_PRESS)
	{
	}
	if (key == GLFW_KEY_BACKSLASH && action == GLFW_PRESS)
	{
		activeAnim->toggleSubpixelSampling();
	}
	if (key == GLFW_KEY_LEFT_BRACKET && action == GLFW_PRESS) {
		smoothingFrames = max(1, smoothingFrames-1);
		std::printf("[FFC] Decrease smoothing to %d\n", smoothingFrames);
	}
	if (key == GLFW_KEY_RIGHT_BRACKET && action == GLFW_PRESS) {
		smoothingFrames = min(5, smoothingFrames+1);
		std::printf("[FFC] Increase smoothing to %d\n", smoothingFrames);
	}
	if (calibration) {
		if (key == GLFW_KEY_ENTER && action == GLFW_PRESS) {
			calibration->select();
		}
		if (key == GLFW_KEY_LEFT && (action == GLFW_PRESS || action == GLFW_REPEAT)) {
			mods & GLFW_MOD_SHIFT ? calibration->goLeft(10) : calibration->goLeft(1);
		}
		if (key == GLFW_KEY_RIGHT && (action == GLFW_PRESS || action == GLFW_REPEAT)) {
			mods & GLFW_MOD_SHIFT ? calibration->goRight(10) : calibration->goRight(1);
		}
		if (key == GLFW_KEY_C && action == GLFW_PRESS) {
			calibration->printCalibration();
			delete calibration;
			calibration = NULL;

			cam->moveTo(stage->getCentroid());
		}
	} else if (key == GLFW_KEY_C && action == GLFW_PRESS) {
		calibration = new CylinderCalibration(stage);
	}
}

void FireflyController::mouseMove(double xPos, double yPos)
{
	if (lastX != -1.0 && moveCamera)
	{
		cam->rotate((lastX-xPos)*CAM_SPEED_HORIZ, (yPos-lastY)*CAM_SPEED_VERT);
	}

	lastX = xPos;
	lastY = yPos;

}

void FireflyController::mouseButtonClick(int button, int action, int mods)
{

	if (button == GLFW_MOUSE_BUTTON_LEFT) {
		cameraAutoSpin = false;
		if (action == GLFW_PRESS)
		{
			moveCamera = true;
			glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_HIDDEN);
		}
		else if (action == GLFW_RELEASE)
		{
			moveCamera = false;
			glfwSetInputMode(window, GLFW_CURSOR, GLFW_CURSOR_NORMAL);
		}
	} else if (button == GLFW_MOUSE_BUTTON_RIGHT)
	{
		if (action == GLFW_PRESS)
		{
			cameraAutoSpin = true;
		}
	} else if (button == GLFW_MOUSE_BUTTON_MIDDLE && action == GLFW_PRESS)
	{
	}
}

void FireflyController::mouseScroll(double xOffset, double yOffset)
{
	if (yOffset < 0)
		cam->zoomOut();
	if (yOffset > 0)
		cam->zoomIn();
}

void FireflyController::error_callback(int error, const char* description)
{
	log(string(description));
}

void FireflyController::route_key_callback(GLFWwindow* window, int key, int scancode, int action, int mods) {
	FireflyController::windowControllers[window]->keyPress(key, scancode, action, mods);
}

void FireflyController::route_mouse_move_callback(GLFWwindow* window, double xPos, double yPos) {
	FireflyController::windowControllers[window]->mouseMove(xPos, yPos);
}

void FireflyController::route_mouse_button_callback(GLFWwindow* window, int button, int action, int mods) {
	FireflyController::windowControllers[window]->mouseButtonClick(button, action, mods);
}

void FireflyController::route_mouse_scroll_callback(GLFWwindow* window, double xOffset, double yOffset) {
	FireflyController::windowControllers[window]->mouseScroll(xOffset, yOffset);
}
