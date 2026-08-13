/*
 * PostSparkleFilter.cpp
 *
 *  Created on: Feb 11, 2016
 *      Author: d
 */

#include <animation/pixelshaders/SparkleShader.h>
#include <math.h>
#include <stdlib.h>
#include "FireflyUtils.h"
#include <string>

SparkleShader::~SparkleShader() {
	// TODO Auto-generated destructor stub
}

glm::vec4 SparkleShader::renderPixel(glm::vec3 pos, glm::vec4 color, ArbitraryMap* details) {
	if(rand() % SPARKLE_CREATE_CHANCE == 0)
		sparklesToCreate++;

	char* key = getStateKey();

	if(!(details->hasKey(key))) {
		details->setInt(key, 0);
	}

	if(details->getInt(key) == 0 && sparklesToCreate > 0) {
		details->setInt(key, 1);
		sparklesToCreate--;
	}

	int ss = details->getInt(key);

	double intensity;
	if(ss <= SPARKLE_RISE) {
		intensity = (double)ss/SPARKLE_RISE;
	} else {
		ss -= SPARKLE_RISE;
		intensity = 1.0-(double)ss/SPARKLE_FALL;
	}

	intensity = intensity*(SPARKLE_MAX-SPARKLE_MIN) + SPARKLE_MIN;
	applyIntensity(&color, intensity);

	if(ss > 0) {
		details->setInt(key, ss+1);
	}
	if(ss == SPARKLE_RISE + SPARKLE_FALL) {
		details->setInt(key, 0);
	}

	return color;
}

void SparkleShader::applyIntensity(glm::vec4 *color, double intensity) {
	double maxColor;
	if(color->r > color->g && color->r > color->b) {
		maxColor = color->r;
	} else if (color->g > color->b && color->g > color->r) {
		maxColor = color->g;
	} else {
		maxColor = color->b;
	}

	double maxMultiplier = 1.0/maxColor;
	double multiplier = std::max(0.0, std::min(maxMultiplier, (intensity*SPARKLE_BRIGHTNESS)+1.0));

	//HACK
	multiplier = intensity;
	color->r *= multiplier;
	color->g *= multiplier;
	color->b *= multiplier;
}

char* SparkleShader::getStateKey() {
	return "sparkleState";
	char* key = new char[14];
	key = "SparkleState00";

	key[12] = (char)(id % 256);
	key[13] = (char)(id/256);

	return key;
}
