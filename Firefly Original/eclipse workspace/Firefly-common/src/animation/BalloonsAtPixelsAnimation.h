/*
 * BalloonsAtPixelsAnimation.h
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BALLOONSATPIXELSANIMATION_H_
#define SRC_ANIMATION_BALLOONSATPIXELSANIMATION_H_

#include <animation/BalloonsAnimation.h>

class BalloonsAtPixelsAnimation: public BalloonsAnimation {
protected:
	virtual SpherePrim* newSphere();

public:
	BalloonsAtPixelsAnimation(
			PixelStage* stage,
			AColorPalette *palette = new RandomSaturatedPalette(),
			AColorScheme *scheme = new SolidColorsScheme())
			: BalloonsAnimation(stage, palette, scheme)
	{
		subpixelSampling = false;
	};
	virtual ~BalloonsAtPixelsAnimation();

};

#endif /* SRC_ANIMATION_BALLOONSATPIXELSANIMATION_H_ */
