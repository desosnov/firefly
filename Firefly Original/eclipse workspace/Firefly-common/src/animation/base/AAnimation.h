/*
 * Animation.h
 *
 *  Created on: Mar 5, 2016
 *      Author: Denis
 */

#ifndef SRC_COMMON_ANIMATIONS_ANIMATION_H_
#define SRC_COMMON_ANIMATIONS_ANIMATION_H_

#include <animation/base/AColorPalette.h>
#include <animation/base/AColorScheme.h>
#include <animation/base/APixelShader.h>
#include <animation/base/ArbitraryMap.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>
#include <animation/colors/schemes/SolidColorsScheme.h>
#include "stage/PixelStage.h"
#include "stage/Pixel.h"
#include <map>
#include <vector>

#define SUBPIXEL_RADIUS_RATIO 1.0
#define SUBPIXEL_ORIGINAL_WEIGHT 0.25
#define SUBPIXEL_WEIGHT ((1.0-SUBPIXEL_ORIGINAL_WEIGHT)/6.0)

class AAnimation {
protected:
	PixelStage* stage;
	AColorPalette* palette;
	AColorScheme* colorScheme;
	std::vector<ArbitraryMap> pixelDetails;
	std::vector<APixelShader*> pixelShaders;

	double startTime = 0.0;
	bool subpixelSampling = false;
	double subpixelDist;

	virtual void initInternal() =0;
	virtual void updateInternal(double time) =0;
	virtual glm::vec4 renderPixelInternal(glm::vec3 pixelPos, ArbitraryMap* details) =0;
	virtual glm::vec4 blendColors(glm::vec4 underColor, glm::vec4 overColor);

public:
	AAnimation(PixelStage* pixelStage,
			AColorPalette* palette = new RandomSaturatedPalette(),
			AColorScheme* colorScheme = new SolidColorsScheme());
	virtual ~AAnimation();

	void init(double time);
	void update(double time);
	void render(double time);
	glm::vec4 renderPixel(glm::vec3 pixelPos, ArbitraryMap* details);

	virtual void beginWrappingUp() =0;
	virtual bool readyForNextAnimation() =0;
	virtual bool finished() =0;

	void addShader(APixelShader* shader);
	void resetShaders();
	void setColorScheme(AColorScheme *newScheme);
	void setColorPalette(AColorPalette *newPalette);

	bool toggleSubpixelSampling();
};

#endif /* SRC_COMMON_ANIMATIONS_ANIMATION_H_ */
