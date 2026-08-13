/*
 * Animation.cpp
 *
 *  Created on: Mar 5, 2016
 *      Author: Denis
 */

#include <animation/base/AAnimation.h>
#include "FireflyUtils.h"

AAnimation::AAnimation(
		PixelStage* pixelStage,
		AColorPalette* palette,
		AColorScheme* colorScheme)
	: stage(pixelStage), palette(palette), colorScheme(colorScheme) {
	pixelDetails.resize(stage->pixelsLen);
	for(int p = 0; p < stage->pixelsLen; p++) {
		pixelDetails[p].setInt("pixelIndex", p);
	}
	subpixelDist = pixelStage->getPixelRadius() * SUBPIXEL_RADIUS_RATIO;
}

AAnimation::~AAnimation() {
	// TODO Auto-generated destructor stub
}

glm::vec4 AAnimation::blendColors(glm::vec4 underColor, glm::vec4 overColor) {
	float outAlpha = overColor.a + underColor.a * (1.0f - overColor.a);
	if(outAlpha == 0.0) {
		return glm::vec4(0.0, 0.0, 0.0, 0.0);
	}

	glm::vec3 outRGB = (glm::vec3(overColor) * overColor.a
			+ (1.0f - overColor.a) * glm::vec3(underColor) * underColor.a)
			/ outAlpha;
	return glm::vec4(outRGB, outAlpha);
}

void AAnimation::init(double time) {
	startTime = time;
	initInternal();
}

void AAnimation::update(double time) {
	updateInternal(time-startTime);
}

void AAnimation::render(double time) {
	updateInternal(time-startTime);

	for (int p = 0; p < stage->pixelsLen; p++)
	{
		pixelDetails[p].setInt("shaderIndex", -1);
		glm::vec4 pixelColor = renderPixel(stage->pixels[p].getPos(), &pixelDetails[p]);
		stage->pixels[p].setColor(glm::vec3(pixelColor.r*pixelColor.a, pixelColor.g*pixelColor.a, pixelColor.b*pixelColor.a));
	}
}

glm::vec4 AAnimation::renderPixel(glm::vec3 pixelPos, ArbitraryMap* details) {
	glm::vec4 pixelColor;

	if(subpixelSampling) {
		glm::vec3 subpixelPos = pixelPos;

		subpixelPos.x += subpixelDist;
		pixelColor = renderPixelInternal(subpixelPos, details);
		subpixelPos.x -= subpixelDist*2;
		pixelColor += renderPixelInternal(subpixelPos, details);
		subpixelPos.x += subpixelDist;

		subpixelPos.y += subpixelDist;
		pixelColor += renderPixelInternal(subpixelPos, details);
		subpixelPos.y -= subpixelDist*2;
		pixelColor += renderPixelInternal(subpixelPos, details);
		subpixelPos.y += subpixelDist;

		subpixelPos.z += subpixelDist;
		pixelColor += renderPixelInternal(subpixelPos, details);
		subpixelPos.z -= subpixelDist*2;
		pixelColor += renderPixelInternal(subpixelPos, details);
		subpixelPos.z += subpixelDist;

		pixelColor *= (float)SUBPIXEL_WEIGHT;
		pixelColor += (float)SUBPIXEL_ORIGINAL_WEIGHT * renderPixelInternal(pixelPos, details);
	} else {
		pixelColor = renderPixelInternal(pixelPos, details);
	}

	for(int s = 0; s < pixelShaders.size(); s++) {
		if(pixelShaders[s] != NULL) {
			pixelColor = pixelShaders[s]->renderPixel(pixelPos, pixelColor, details);
		}
	}

	return pixelColor;
}

void AAnimation::addShader(APixelShader* shader) {
	pixelShaders.push_back(shader);
}

void AAnimation::resetShaders() {
	pixelShaders.clear();
}

void AAnimation::setColorScheme(AColorScheme* newScheme) {
	colorScheme = newScheme;
}

void AAnimation::setColorPalette(AColorPalette* newPalette) {
	palette = newPalette;
}

bool AAnimation::toggleSubpixelSampling() {
	subpixelSampling = !subpixelSampling;
	log("ANIM Set subpixel sampling to " + to_string(subpixelSampling));
	return subpixelSampling;
}
