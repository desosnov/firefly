/*
 * GradientToBlackScheme.h
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOBLACKSCHEME_H_
#define SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOBLACKSCHEME_H_

#include <animation/base/AColorScheme.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>
#include <animation/colors/patterns/TwoColorGradientPattern.h>

class GradientToBlackScheme: public AColorScheme {
public:
	GradientToBlackScheme(AColorPalette *palette = new RandomSaturatedPalette())
		: AColorScheme(palette)
	{};
	virtual ~GradientToBlackScheme();

	virtual AColorPattern* nextColor();
};

#endif /* SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOBLACKSCHEME_H_ */
