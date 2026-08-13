/*
 * PixelStrip.h
 *
 *  Created on: Jan 11, 2016
 *      Author: Denis
 */

#ifndef PIXELSTAGE_H_
#define PIXELSTAGE_H_

#include <math.h>
#include "Serial.h"
#include <map>
#include <glm/glm.hpp>

#include "stage/Pixel.h"

#define CYL_DIAM (2.54*5.75)
#define CYL_STRIP_HEIGHT 1.25
#define CYL_METER_STRIPS 10
#define CYL_LEDS_PER_METER 144
#define CYL_LED_DIST (100.0/CYL_LEDS_PER_METER)
#define CYL_LEDS (CYL_METER_STRIPS * CYL_LEDS_PER_METER)
#define SCALE_FACTOR (0.2/2.54) // # of cm in one unit
#define CYL_HEIGHT CYL_METER_STRIPS*100.0/sqrt(pow(CYL_STRIP_HEIGHT,2) + pow(CYL_DIAM*M_PI,2))*CYL_STRIP_HEIGHT
#define CYL_PIXEL_RADIUS (CYL_LED_DIST*SCALE_FACTOR/2.0)
#define CYL_PIXEL_RENDER_RADIUS (CYL_PIXEL_RADIUS*0.9)

#define MILLIAMPS_PER_COLOR 20.0

#define PS_MAX_POWER 9000.0
#define PS_MIN_POWER 500.0
#define PS_DEFAULT_POWER 3000.0
#define PS_POWER_INTERVAL 250.0
#define PS_MAX_BRIGHTNESS 0.45
#define PS_MIN_BRIGHTNESS 0.05
#define PS_DEFAULT_BRIGHTNESS 0.2
#define PS_BRIGHTNESS_MOVE_THRESHOLD 0.02
#define PS_BRIGHTNESS_INTERVAL 0.1

enum PixelStageOption
{
	DEFAULT_CYLINDER = 1,
	DEFAULT_FILLED_CUBE = 2,
	FIREFLY_V1_CYLINDER = 3,
	FIREFLY_V2_CYLINDER = 4
};

class PixelStage {
private:
	double pixelRadius = CYL_PIXEL_RADIUS * SCALE_FACTOR;
	void (*drawStageFunc)();

	float targetPower, brightness;
	float powerDraw;
	int lastReportedBrightness;

	glm::vec3 center;
	bool centerFlag;
	double maxRadius, medianRadius;
	bool maxRadiusFlag, medianRadiusFlag;

	std::map<int,double> cylinderAnchors;

	void generateDefaultCylinder();
	void generateCylinderWithAnchors(std::map<int, double> anchors);
	void generateFireflyV1Cylinder();
	void generateFireflyV2Cylinder();
	void static drawDefaultCylinderWalls();
	void generateFilledCube();

public:
	//TEMP HACK MOVE BACK TO PRIVATE
	int pixelsLen;
	Pixel* pixels;

	PixelStage();
	PixelStage(PixelStageOption option);
	virtual ~PixelStage();

	void renderGL();
	void renderLED(Serial *serial);

	void brightnessUp();
	void brightnessDown();

	float getBrightness();
	float getTargetPower();
	float getPowerDraw();

	glm::vec3 getCentroid();
	double getMaxRadius();
	double getMedianRadius();
	double getPixelRadius();

	std::map<int,double> getAnchors();
	void setAnchors(std::map<int,double> anchors);
};

#endif /* PIXELSTAGE_H_ */
