/*
 * PixelStrip.cpp
 *
 *  Created on: Jan 11, 2016
 *      Author: Denis
 */

#include "stage/PixelStage.h"

#include <stdlib.h>
#include <stdio.h>
#include <iostream>
#include "FireflyUtils.h"

#define _USE_MATH_DEFINES
#include <math.h>

#if defined _WIN32 || defined _WIN64
	#include <GL/gl.h>
#else
	#include <OpenGL/gl.h>
#endif

using namespace std;

PixelStage::PixelStage() {
	centerFlag = false;
	maxRadiusFlag = false;
	PixelStage(FIREFLY_V2_CYLINDER);
}

PixelStage::PixelStage(PixelStageOption option)
{
	brightness = PS_DEFAULT_BRIGHTNESS;
	targetPower = PS_DEFAULT_POWER;

	switch(option)
	{//TODO: These should obviously be child classes
		case DEFAULT_CYLINDER:
			generateDefaultCylinder();
			break;
		case DEFAULT_FILLED_CUBE:
			generateFilledCube();
			break;
		case FIREFLY_V1_CYLINDER:
			generateFireflyV1Cylinder();
			break;
		case FIREFLY_V2_CYLINDER:
			generateFireflyV2Cylinder();
			break;
		default:
			break;
	};
}

PixelStage::~PixelStage() {
	// TODO Auto-generated destructor stub
}

void PixelStage::generateCylinderWithAnchors(map<int, double> anchors)
{
	centerFlag = false;
	maxRadiusFlag = false;
	cylinderAnchors = anchors;
	drawStageFunc = &drawDefaultCylinderWalls;
	pixels = new Pixel[CYL_LEDS];
	pixelsLen = CYL_LEDS;
	pixelRadius = CYL_PIXEL_RENDER_RADIUS;

	double radialInterval;
	double verticalInterval = CYL_STRIP_HEIGHT/(2*M_PI);

	int prevAnchor = 0, nextAnchor;
	double prevAnchorRadial = 0.0, nextAnchorRadial;
	int pixel = 0;
	double curRadial = 0.0;
	for(std::map<int, double>::iterator anchor_iter = anchors.begin(); anchor_iter != anchors.end(); anchor_iter++) {
		nextAnchor = anchor_iter->first;
		nextAnchorRadial = anchor_iter->second;
		radialInterval = (nextAnchorRadial-prevAnchorRadial)/(nextAnchor-prevAnchor);

		for(pixel = prevAnchor; pixel < nextAnchor; pixel++) {
			glm::vec3 pixelPos = glm::vec3(
					0.5 + cos(curRadial)*CYL_DIAM/2.0*SCALE_FACTOR,
					0.5 + sin(curRadial)*CYL_DIAM/2.0*SCALE_FACTOR,
					0.0 + curRadial*verticalInterval*SCALE_FACTOR);
			pixels[pixel] = Pixel(pixelPos);
			curRadial += radialInterval;
		}

		prevAnchor = nextAnchor;
		prevAnchorRadial = nextAnchorRadial;
	}

	glm::vec3 pixelPos = glm::vec3(
			0.5 + cos(curRadial)*CYL_DIAM/2.0*SCALE_FACTOR,
			0.5 + sin(curRadial)*CYL_DIAM/2.0*SCALE_FACTOR,
			0.0 + curRadial*verticalInterval*SCALE_FACTOR);
	pixels[pixel] = Pixel(pixelPos);
}

void PixelStage::generateDefaultCylinder() {
	centerFlag = false;
	maxRadiusFlag = false;

	double lengthOfOneLoop = sqrt(pow(CYL_STRIP_HEIGHT, 2) + pow(CYL_DIAM*M_PI,2));
	double ledsPerLoop = lengthOfOneLoop / CYL_LED_DIST;

	map<int, double> anchors;
	anchors[CYL_LEDS-1] = 2.0*M_PI/ledsPerLoop*CYL_LEDS;

	generateCylinderWithAnchors(anchors);
}

void PixelStage::generateFireflyV1Cylinder() {
	centerFlag = false;
	maxRadiusFlag = false;

	map<int, double> anchors;
	anchors[52] = 6.30062;
	anchors[103] = 12.5297;
	anchors[141] = 17.1286;
	anchors[144] = 17.3632;
	anchors[156] = 18.8118;
	anchors[208] = 25.1225;
	anchors[215] = 25.9733;
	anchors[216] = 26.1449;
	anchors[259] = 31.3615;
	anchors[287] = 34.7449;
	anchors[288] = 35.0065;
	anchors[310] = 37.6806;
	anchors[359] = 43.5766;
	anchors[360] = 43.7381;
	anchors[362] = 43.9912;
	anchors[414] = 50.2718;
	anchors[431] = 52.3182;
	anchors[432] = 52.5797;
	anchors[465] = 56.6009;
	anchors[503] = 61.2098;
	anchors[504] = 61.3714;
	anchors[516] = 62.83;
	anchors[568] = 69.1406;
	anchors[575] = 69.9714;
	anchors[576] = 70.273;
	anchors[619] = 75.4496;
	anchors[647] = 78.833;
	anchors[648] = 79.0246;
	anchors[671] = 81.8003;
	anchors[719] = 87.5847;
	anchors[720] = 87.8262;
	anchors[722] = 88.0693;
	anchors[774] = 94.3499;
	anchors[791] = 96.4063;
	anchors[792] = 96.6078;
	anchors[826] = 100.701;
	anchors[863] = 105.178;
	anchors[864] = 105.439;
	anchors[877] = 107;
	anchors[929] = 113.26;
	anchors[935] = 113.98;
	anchors[936] = 114.171;
	anchors[981] = 119.601;
	anchors[1007] = 122.741;
	anchors[1008] = 122.993;
	anchors[1032] = 125.88;
	anchors[1079] = 131.553;
	anchors[1080] = 131.714;
	anchors[1084] = 132.201;
	anchors[1136] = 138.481;
	anchors[1151] = 140.284;

	generateCylinderWithAnchors(anchors);
}

void PixelStage::generateFireflyV2Cylinder() {
	centerFlag = false;
	maxRadiusFlag = false;

	map<int, double> anchors;
	anchors[0] = -0.03;
	anchors[67] = 6.22355;
	anchors[71] = 6.60606;
	anchors[72] = 6.74919;
	anchors[133] = 12.4561;
	anchors[143] = 13.4032;
	anchors[144] = 13.5384;
	anchors[199] = 18.6804;
	anchors[215] = 20.1824;
	anchors[216] = 20.3376;
	anchors[266] = 25.0139;
	anchors[287] = 26.9816;
	anchors[288] = 27.1267;
	anchors[333] = 31.3475;
	anchors[359] = 33.7708;
	anchors[360] = 33.9159;
	anchors[400] = 37.671;
	anchors[431] = 40.57;
	anchors[432] = 40.7151;
	anchors[467] = 43.9946;
	anchors[503] = 47.3692;
	anchors[504] = 47.5043;
	anchors[534] = 50.3181;
	anchors[575] = 54.1584;
	anchors[576] = 54.2935;
	anchors[601] = 56.6417;
	anchors[647] = 60.9476;
	anchors[648] = 61.0927;
	anchors[668] = 62.9652;
	anchors[719] = 67.7567;
	anchors[720] = 68.0219;
	anchors[733] = 69.2385;
	anchors[791] = 74.6759;
	anchors[792] = 74.8111;
	anchors[800] = 75.5621;
	anchors[863] = 81.4651;
	anchors[864] = 81.6002;
	anchors[867] = 81.8856;
	anchors[935] = 88.2443;
	anchors[936] = 88.3885;
	anchors[1001] = 94.4727;
	anchors[1007] = 95.0435;
	anchors[1008] = 95.1786;
	anchors[1068] = 100.786;
	anchors[1078] = 101.738;
	anchors[1079] = 101.823;
	anchors[1080] = 101.958;
	anchors[1081] = 102.053;
	anchors[1083] = 102.243;
	anchors[1087] = 102.614;
	anchors[1135] = 107.1;
	anchors[1151] = 108.602;
	anchors[1152] = 108.747;
	anchors[1202] = 113.433;
	anchors[1223] = 115.391;
	anchors[1224] = 115.536;
	anchors[1269] = 119.757;
	anchors[1295] = 122.19;
	anchors[1296] = 122.335;
	anchors[1336] = 126.08;
	anchors[1367] = 128.979;
	anchors[1368] = 129.125;
	anchors[1403] = 132.394;
	anchors[1439] = 135.779;

	generateCylinderWithAnchors(anchors);
}

std::map<int,double> PixelStage::getAnchors() {
	return cylinderAnchors;
}

void PixelStage::setAnchors(std::map<int,double> anchors) {
	generateCylinderWithAnchors(anchors);
}

#define CYL_SLICES 24
#define CYL_STACKS 24
#define CYL_DARKNESS 0.1
#define CYL_ALPHA 0.6
void PixelStage::drawDefaultCylinderWalls()
{
	double lowZ = 0.0;
	double highZ = CYL_HEIGHT*SCALE_FACTOR;
	double dir = 0.0;

	double radius = (CYL_DIAM/2.0-CYL_PIXEL_RADIUS)*SCALE_FACTOR;

	glColor4f(CYL_DARKNESS, CYL_DARKNESS, CYL_DARKNESS, CYL_ALPHA);
	glBegin(GL_TRIANGLE_STRIP);
	for(int s = 0; s < CYL_SLICES; s++)
	{
		glVertex3f(0.5 + cos(dir)*radius, 0.5 + sin(dir)*radius, lowZ);
		glVertex3f(0.5 + cos(dir)*radius, 0.5 + sin(dir)*radius, highZ);
		dir += M_PI*2.0/CYL_SLICES;
	}
	glVertex3f(0.5 + cos(dir)*radius, 0.5 + sin(dir)*radius, lowZ);
	glVertex3f(0.5 + cos(dir)*radius, 0.5 + sin(dir)*radius, highZ);
	glEnd();
}

#define CUBE_LEDS_PER_SIDE 15
#define CUBE_SIDE_LENGTH 1.0
#define CUBE_LEDS (CUBE_LEDS_PER_SIDE*CUBE_LEDS_PER_SIDE*CUBE_LEDS_PER_SIDE)
#define CUBE_PIXEL_DISTANCE (CUBE_SIDE_LENGTH/(double)CUBE_LEDS_PER_SIDE)
#define CUBE_PIXEL_SPACE_RATIO 0.2

void PixelStage::generateFilledCube()
{
	centerFlag = false;
	maxRadiusFlag = false;

	drawStageFunc = NULL;
	pixels = new Pixel[CUBE_LEDS];
	pixelsLen = CUBE_LEDS;
	pixelRadius = CUBE_PIXEL_DISTANCE*CUBE_PIXEL_SPACE_RATIO;

	int pi = 0;
	for (int xi = 0; xi < CUBE_LEDS_PER_SIDE; xi++)
	{
		for (int yi = 0; yi < CUBE_LEDS_PER_SIDE; yi++)
		{
			for (int zi = 0; zi < CUBE_LEDS_PER_SIDE; zi++)
			{
				pixels[pi] = Pixel(glm::vec3(
						xi*CUBE_PIXEL_DISTANCE,
						yi*CUBE_PIXEL_DISTANCE,
						zi*CUBE_PIXEL_DISTANCE));
				pi++;
			}
		}
	}
}

void PixelStage::renderGL()
{
	for (int p = 0; p < pixelsLen; p++)
	{
		pixels[p].render(pixelRadius);

		glm::vec3 color = pixels[p].getColor();
	}

	if (drawStageFunc)
		(this->drawStageFunc)();
}

int maxNum(int a, int b) {
	if (a > b) {
		return a;
	} else {
		return b;
	}
}

void PixelStage::renderLED(Serial *serial) {
	if(powerDraw > 0.0 && fabs(targetPower/powerDraw-1.0) > PS_BRIGHTNESS_MOVE_THRESHOLD) {
		int oldBrightness = (int)floor(brightness*5.0);
		brightness = fmax(PS_MIN_BRIGHTNESS, fmin(PS_MAX_BRIGHTNESS,
				brightness * ((targetPower/powerDraw-1.0)*PS_BRIGHTNESS_INTERVAL+1.0)));
		if((int)floor(brightness*5.0) != oldBrightness && (int)round(brightness*5.0) != lastReportedBrightness) {
			std::printf("[PS] Auto-brightness at %.2f%%\n", brightness*100.0);
			lastReportedBrightness = (int)round(brightness*5.0);
		}
/*		if(powerDraw > targetPower) {
			brightness = fmax(PS_MIN_BRIGHTNESS, fmin(PS_MAX_BRIGHTNESS, brightness * (1.0-PS_BRIGHTNESS_INTERVAL)));
		} else if(powerDraw < targetPower) {
			brightness = fmax(PS_MIN_BRIGHTNESS, fmin(PS_MAX_BRIGHTNESS, brightness * (1.0+PS_BRIGHTNESS_INTERVAL)));
		}*/
	}
	powerDraw = 0.0;

	char pixelOut[3*pixelsLen+1];
	pixelOut[3*pixelsLen] = '\0';
	glm::vec3 color;
	int r, g, b;

	for(int pi = 0; pi < pixelsLen; pi++) {
		color = pixels[pi].getColor();
		r = max(min((int)round(color.r*brightness*255),255),0);
		g = max(min((int)round(color.g*brightness*255),255),0);
		b = max(min((int)round(color.b*brightness*255),255),0);

		pixelOut[pi*3] = (char)r;
		pixelOut[pi*3+1] = (char)g;
		pixelOut[pi*3+2] = (char)b;

		powerDraw += ((float)(r+g+b))/255.0 * MILLIAMPS_PER_COLOR;
	}

	serial->write(pixelOut, 3*pixelsLen);
}

glm::vec3 PixelStage::getCentroid()
{
	if(centerFlag) {
		return center;
	}

	center = glm::vec3(0.0, 0.0, 0.0);
	for (int p = 0; p < pixelsLen; p++)
	{
		center += pixels[p].getPos();
	}

	center /= (double)pixelsLen;
	centerFlag = true;
	return center;
}

double PixelStage::getMaxRadius() {
	if (maxRadiusFlag) {
		return maxRadius;
	}

	if(!centerFlag)
		getCentroid();

	maxRadius = 0.0;
	for(int pi = 0; pi < pixelsLen; pi++) {
		double dist = glm::length(center-pixels[pi].getPos());
		if(dist > maxRadius)
			maxRadius = dist;
	}
	maxRadiusFlag = true;
	return maxRadius;
}

double PixelStage::getMedianRadius() {
	return 0.0;
}

double PixelStage::getPixelRadius() {
	return CYL_PIXEL_RADIUS;
}

void PixelStage::brightnessUp() {
	targetPower = fmin(targetPower + PS_POWER_INTERVAL, PS_MAX_POWER);
	std::printf("Target power use: %.2f | Cur brightness: %.2f\n", targetPower, brightness);
}

void PixelStage::brightnessDown() {
	targetPower = fmax(targetPower - PS_POWER_INTERVAL, PS_MIN_POWER);
	std::printf("Target power use: %.2f | Cur brightness: %.2f\n", targetPower, brightness);
}

float PixelStage::getBrightness() {
	return brightness;
}

float PixelStage::getTargetPower() {
	return targetPower;
}

float PixelStage::getPowerDraw() {
	return powerDraw;
}
