/*
 * SpheresAnimation.cpp
 *
 *  Created on: Feb 8, 2016
 *      Author: d
 */

#include <animation/SpheresAnimation.h>
#include "FireflyUtils.h"
#include <stdio.h>

#define _USE_MATH_DEFINES
#include <math.h>

void SpheresAnimation::initInternal() {
	preWrapup = false;
	wrappingUp = false;
	stillRendering = true;
	nextAnimationFlag = false;
	finishedFlag = false;

	std::vector<glm::vec4> colors;
	while(colors.size() < SA_NUM_COLORS) {
		colors.push_back(palette->nextColor());
	}

	ringRadiusOffset = rand(0.0, 2*M_PI);
	colorBlendOffset = rand(0.0, 2*M_PI);

	if(spherePrimitive) delete spherePrimitive;
	spherePrimitive = new ConcentricSpheresPrim(
			stage->getCentroid(),
			0.0,
			0.0,
			colors,
			0.0);

	setUpNextMove(0.0);
	setUpNextColorChange(0.0);
}

void SpheresAnimation::updateInternal(double time) {
	spherePrimitive->sizeInRings = time*SA_RINGS_PER_SECOND_AVG;
	spherePrimitive->ringRadius =
			SA_RING_SIZE_AVG + SA_RING_SIZE_RANGE * cos(ringRadiusOffset + time / SA_RING_SIZE_CYCLE * 2 * M_PI);
	spherePrimitive->colorBlendingFactor = fmax(-0.9999, sin(colorBlendOffset + time / SA_COLOR_BLEND_CYCLE * 2 * M_PI));

	if(time > nextColorChange) {
		setUpNextColorChange(time);
	}

	if(posEasingFunc->finished()) {
		setUpNextMove(time);
	}

	posEasingFunc->update(time);
	colorEasingFunc->update(time);

	if(wrappingUp) {
		if(!stillRendering) {
			finishedFlag = true;
		} else {
			stillRendering = false;
		}
	}

	if(preWrapup && spherePrimitive->colorBlendingFactor > SA_WRAPUP_MIN_CBF
			&& spherePrimitive->ringRadius * spherePrimitive->sizeInRings > 2.0*stage->getMaxRadius()) {
		preWrapup = false;
		wrappingUp = true;
		spherePrimitive->finalRing = ceil(spherePrimitive->sizeInRings)+3;
	}
}

glm::vec4 SpheresAnimation::renderPixelInternal(glm::vec3 pos, ArbitraryMap* details) {
	glm::vec4 col = spherePrimitive->renderPixel(pos, details);
	if(wrappingUp) {
		if(col.a < 1.0f) {
			nextAnimationFlag = true;
		}
		if(col.a > 0.0f){
			stillRendering = true;
		}
	}
	return col;
}

void SpheresAnimation::beginWrappingUp() {
	preWrapup = true;
}

bool SpheresAnimation::readyForNextAnimation() {
	return nextAnimationFlag;
}

bool SpheresAnimation::finished() {
	return finishedFlag;
}

void SpheresAnimation::setUpNextMove(double time) {
	glm::vec3 newPos = glm::vec3(rand(-1.0, 1.0), rand(-1.0, 1.0), rand(-1.0, 1.0));
	newPos = stage->getCentroid() + (newPos*(float)(stage->getMaxRadius()));

	double endtime = time + rand1()*SA_CENTER_MOVE_TIME_RANGE+SA_CENTER_MOVE_TIME_MIN;

	delete posEasingFunc;
	posEasingFunc = new CosineEase3D(
			time,
			endtime,
			spherePrimitive->centerPos,
			newPos);
	posEasingFunc->bindValue(&(spherePrimitive->centerPos));

	printf("[SA] Moving at t = %.2f to %.2f from %.2f %.2f %.2f to %.2f %.2f %.2f\n",
			time, endtime,
			spherePrimitive->centerPos.x, spherePrimitive->centerPos.y, spherePrimitive->centerPos.z,
			newPos.x, newPos.y, newPos.z);
}

void SpheresAnimation::setUpNextColorChange(double time) {
	double colorChangeStart = time + (SA_COLOR_CHANGE_INTERVAL-SA_COLOR_CHANGE_TIME) * rand1();
	double colorChangeEnd = colorChangeStart + SA_COLOR_CHANGE_TIME;

	int	colorChangeIndex = rand() % spherePrimitive->colors.size();
	glm::vec4 bestColor, nextColor;
	float bestRange = 0.0, nextRange;
	for(int i = 0; i < SA_COLOR_RETRIES; i++) {
		nextColor = palette->nextColor();
		nextRange = 0.0;
		for(int ci = 0; ci < spherePrimitive->colors.size(); ci++) {
			if(ci != colorChangeIndex) {
				nextRange = fmax(nextRange, glm::distance(spherePrimitive->colors[ci], nextColor));
			}
		}
		if(nextRange > bestRange) {
			bestRange = nextRange;
			bestColor = nextColor;
		}
		if(bestRange > SA_COLOR_RANGE_THRESHOLD) {
			break;
		}
	}

	delete colorEasingFunc;
	colorEasingFunc = new CosineEase4D(
			colorChangeStart,
			colorChangeEnd,
			spherePrimitive->colors[colorChangeIndex],
			bestColor);
	colorEasingFunc->bindValue(&(spherePrimitive->colors[colorChangeIndex]));

	printf("[SA] Next color change t = %.2f to %.2f. Color blending factor %.2f\n",
			colorChangeStart, colorChangeEnd, spherePrimitive->colorBlendingFactor);

	nextColorChange = time + SA_COLOR_CHANGE_INTERVAL;
}

void SpheresAnimation::shuffleColors()
{
	for (int colorIndex = 0; colorIndex < spherePrimitive->colors.size(); colorIndex++)
		spherePrimitive->colors[colorIndex] = palette->nextColor();
}
