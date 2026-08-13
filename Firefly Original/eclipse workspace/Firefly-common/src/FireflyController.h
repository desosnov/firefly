/*
 * FireflyController.h
 *
 *  Created on: Feb 7, 2016
 *      Author: Denis
 */

#ifndef SRC_COMMON_FIREFLYCONTROLLER_H_
#define SRC_COMMON_FIREFLYCONTROLLER_H_

// Standards
#include <stdlib.h>
#include <stdio.h>
#include <iostream>
#include <unistd.h>
#include <math.h>
#include <string>
#include <sstream>
#include <map>
#include <glm/glm.hpp>

// GLFW
#define GLFW_INCLUDE_GLU
#include <GLFW/glfw3.h>

// My stuff
#include "Camera.h"
#include "stage/Pixel.h"
#include "stage/PixelStage.h"
#include "Serial.h"
#include "FireflyUtils.h"
#include <animation/SpheresAnimation.h>
#include <animation/BalloonsAnimation.h>
#include "stage/CylinderCalibration.h"

#define DEFAULT_WINDOW_WIDTH 1920 //1920
#define DEFAULT_WINDOW_HEIGHT 1280 // 1280
#define DEFAULT_WINDOW_TITLE "Firefly Controller"

#define FFC_MAX_SMOOTHING 1
#define FFC_MIN_SMOOTHING 1

class FireflyController {
private:
	GLFWwindow* window;
	static map<GLFWwindow*, FireflyController*> windowControllers;

	Camera* cam;
	PixelStage* stage;
	CylinderCalibration *calibration;
	AAnimation *activeAnim;
	Serial* serial;

	double lastX, lastY;
	bool moveCamera;
	bool cameraAutoSpin;

	double nextUpdateTime = 5.0;
	int frameCount = 0, smoothingFrames = 1;
	double timeAnchor = 0.0, lastFrame = 0.0, processTime = 0.0, outputTime = 0.0, lastProcessTime = 0.0, lastOutputTime = 0.0;
	double cumulativePowerDraw = 0.0;

	GLFWwindow* initGL(const char* title, int width, int height);
	void clearStage();
	void render(GLFWwindow* window, Serial* serial, double time);

public:
	FireflyController(const char* title, int width, int height, const char* serialPort, PixelStageOption stageType);
	virtual ~FireflyController();

	void start();

	void keyPress(int key, int scancode, int action, int mods);
	void mouseMove(double xPos, double yPos);
	void mouseButtonClick(int button, int action, int mods);
	void mouseScroll(double xOffset, double yOffset);

	static void error_callback(int error, const char* description);
	static void route_key_callback(GLFWwindow* window, int key, int scancode, int action, int mods);
	static void route_mouse_move_callback(GLFWwindow* window, double xPos, double yPos);
	static void route_mouse_button_callback(GLFWwindow* window, int button, int action, int mods);
	static void route_mouse_scroll_callback(GLFWwindow* window, double xOffset, double yOffset);
};

#endif /* SRC_COMMON_FIREFLYCONTROLLER_H_ */
