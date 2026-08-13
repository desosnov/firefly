/*
 * SolidColorsScheme.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_SCHEMES_SOLIDCOLORSSCHEME_H_
#define SRC_ANIMATION_COLORS_SCHEMES_SOLIDCOLORSSCHEME_H_

#include <animation/base/AColorScheme.h>
#include <animation/colors/patterns/SolidPattern.h>

class SolidColorsScheme: public AColorScheme {
public:
	SolidColorsScheme(AColorPalette *palette = new RandomSaturatedPalette())
		: AColorScheme(palette)
	{};
	virtual ~SolidColorsScheme();

	virtual AColorPattern* nextColor();
};

#endif /* SRC_ANIMATION_COLORS_SCHEMES_SOLIDCOLORSSCHEME_H_ */
