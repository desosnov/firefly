/*
 * WorldOfBalloonsAnimation.cpp
 *
 *  Created on: Aug 9, 2016
 *      Author: Denis
 */

#include <animation/BalloonsAnimation.h>
#include "FireflyUtils.h"

void BalloonsAnimation::initInternal() {
	wrappingUp = false;
	nextAnimationFlag = false;
	finishedFlag = false;
	nextAnimationTime = 0.0;
	finishTime = 0.0;
	numSpheres = 0;
	spheres.clear();
	growthEases.clear();
	wrapUpEases.clear();

	while(growthEases.size() < WOB_SIMULT_SPHERES_GROWING) {
		addSphere(0.0);
	}
}

void BalloonsAnimation::updateInternal(double time) {
	for(std::vector<AEasingFunction1D*>::iterator iter = growthEases.begin(); iter != growthEases.end(); iter++) {
		AEasingFunction1D* ease = *iter;
		ease->update(time);

		if(ease->finished() && !wrappingUp) {
			addSphere(time);
			growthEases.erase(iter);
		}
	}

	if(wrappingUp) {
		if(wrapUpEases.size() == 0) {
			double moveStart = time, moveStartInterval = WOB_WRAPUP_DURATION / spheres.size();
			nextAnimationTime = time + WOB_WRAPUP_DURATION;
			finishTime = time + WOB_WRAPUP_DURATION + WOB_WRAPUP_MOVE_TIME;

			for(std::vector<SpherePrim*>::reverse_iterator iter = spheres.rbegin(); iter != spheres.rend(); iter++) {
				SpherePrim* sphere = *iter;
				glm::vec3 dirFromCenter = glm::normalize(sphere->centerPos - stage->getCentroid());
				glm::vec3 moveTarget = (float)((WOB_MAX_SIZE + WOB_STAGE_SIZE) * stage->getMaxRadius()) * dirFromCenter + stage->getCentroid();

				CosineEase3D* moveEase = new CosineEase3D(
						moveStart,
						moveStart + WOB_WRAPUP_MOVE_TIME,
						sphere->centerPos,
						moveTarget);
				moveEase->bindValue(&sphere->centerPos);
				wrapUpEases.push_back(moveEase);

				moveStart += moveStartInterval;
			}
		}

		for(std::vector<AEasingFunction3D*>::iterator iter = wrapUpEases.begin(); iter != wrapUpEases.end(); iter++) {
			((AEasingFunction3D*)*iter)->update(time);
		}

		if(time > nextAnimationTime) nextAnimationFlag = true;
		if(time > finishTime) finishedFlag = true;
	}
}

void BalloonsAnimation::addSphere(double time) {
	SpherePrim* sphere = newSphere();
	spheres.push_back(sphere);
	if(spheres.size() > WOB_MAX_SPHERES) {
		spheres.erase(spheres.begin());
	}

	AEasingFunction1D* ease = newGrowthEase(time);
	ease->bindValue(&sphere->radius);
	growthEases.push_back(ease);
}

SpherePrim* BalloonsAnimation::newSphere() {
	glm::vec3 center;
	do {
		center = glm::vec3(
			rand(-1*WOB_STAGE_SIZE, WOB_STAGE_SIZE),
			rand(-1*WOB_STAGE_SIZE, WOB_STAGE_SIZE),
			rand(-1*WOB_STAGE_SIZE, WOB_STAGE_SIZE));
	} while(glm::length(center) > WOB_STAGE_SIZE);

	center = center*(float)(stage->getMaxRadius()) + stage->getCentroid();

	SpherePrim* sphere = new SpherePrim(center, 0.0, colorScheme->nextColor(), numSpheres);
	numSpheres++;
	return sphere;
}

AEasingFunction1D* BalloonsAnimation::newGrowthEase(double time) {
	CosineEase1D* ease = new CosineEase1D(
			time,
			time + rand(WOB_MIN_GROW_TIME, WOB_MAX_GROW_TIME),
			0.0,
			rand(WOB_MIN_SIZE, WOB_MAX_SIZE) * stage->getMaxRadius());
	return ease;
}

glm::vec4 BalloonsAnimation::renderPixelInternal(glm::vec3 pos, ArbitraryMap* details) {
	glm::vec4 pixelColor(0.0, 0.0, 0.0, 0.0);
	glm::vec4 renderColor;

	for(std::vector<SpherePrim*>::reverse_iterator iter = spheres.rbegin(); iter != spheres.rend(); iter++) {
		renderColor = ((SpherePrim*)*iter)->renderPixel(pos, details);
		if(renderColor.a == 0.0f) {
			continue;
		}

		pixelColor = blendColors(renderColor, pixelColor);

		if(pixelColor.a >= WOB_ALPHA_RENDER_CUTOFF) {
			break;
		}
	}

	return pixelColor;
}

void BalloonsAnimation::beginWrappingUp() {
	wrappingUp = true;
}

bool BalloonsAnimation::readyForNextAnimation() {
	return nextAnimationFlag;
}

bool BalloonsAnimation::finished() {
	return finishedFlag;
}
