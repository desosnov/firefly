/*
 * SpheresAnimation.h
 *
 *  Created on: Feb 8, 2016
 *      Author: d
 */

#ifndef SRC_SPHERESANIMATION_H_
#define SRC_SPHERESANIMATION_H_

#include <animation/base/AAnimation.h>
#include <animation/base/AEasingFunction3D.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>
#include <animation/easing/CosineEase3D.h>
#include <animation/easing/CosineEase4D.h>
#include <animation/primitives/ConcentricSpheresPrim.h>
#include <math.h>
#include <glm/glm.hpp>
#include "FireflyUtils.h"

// SA = SpheresAnimation
#define SA_RINGS_PER_SECOND_AVG 0.7 // Rings per second average
#define SA_RINGS_PER_SECOND_RANGE 0.5 // Speed will stay in average +/- this range
#define SA_RINGS_PER_SECOND_CYCLE 50.0 // How long it takes speed to cycle in seconds

#define SA_RING_SIZE_AVG 0.4
#define SA_RING_SIZE_RANGE 0.34
#define SA_RING_SIZE_CYCLE 43.0

#define SA_CENTER_MOVE_TIME_MIN 2.0 // The amount of time it takes to move the center point to its new location
#define SA_CENTER_MOVE_TIME_RANGE 13.0 // will be in the range (MIN, MIN+RANGE)

#define SA_NUM_COLORS 3
#define SA_COLOR_CHANGE_TIME 5.0 // How long it takes a color to crossfade from old to new when changing
#define SA_COLOR_CHANGE_INTERVAL 10.0 // How often a color is faded to a new color
#define SA_COLOR_RETRIES 5
#define SA_COLOR_RANGE_THRESHOLD 0.5

#define SA_COLOR_BLEND_CYCLE 250.0
#define SA_WRAPUP_MIN_CBF -0.75


class SpheresAnimation : public AAnimation {
private:
	ConcentricSpheresPrim *spherePrimitive = NULL;
	double nextColorChange = 0.0;
	AEasingFunction4D *colorEasingFunc = NULL;
	AEasingFunction3D *posEasingFunc = NULL;

	void setUpNextMove(double time);
	void setUpNextColorChange(double time);

	bool preWrapup = false, wrappingUp = false, stillRendering = true;
	bool nextAnimationFlag = false, finishedFlag = false;
	double ringRadiusOffset = 0.0;
	double colorBlendOffset = 0.0;

protected:
	void initInternal();
	void updateInternal(double time);

public:
	SpheresAnimation(PixelStage* stage,
			AColorPalette *palette = new RandomSaturatedPalette(),
			AColorScheme *colorScheme = new SolidColorsScheme())
			: AAnimation(stage, palette, colorScheme)
	{
		subpixelSampling = rand1() > 0.5;
	};

	glm::vec4 renderPixelInternal(glm::vec3 pos, ArbitraryMap* details);

	void beginWrappingUp();
	bool readyForNextAnimation();
	bool finished();

	void shuffleColors();
};

#endif /* SRC_SPHERESANIMATION_H_ */
