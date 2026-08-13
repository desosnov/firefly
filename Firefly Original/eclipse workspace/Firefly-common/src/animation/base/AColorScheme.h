/*
 * ColorScheme.h
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_ABSTRACTCOLORSCHEME_H_
#define SRC_ANIMATION_COLORS_ABSTRACTCOLORSCHEME_H_

#include <animation/base/AColorPalette.h>
#include <animation/base/AColorPattern.h>
#include <animation/base/Timeable.h>
#include <animation/colors/palettes/RandomSaturatedPalette.h>


class AColorScheme : public Timeable {
protected:
	AColorPalette *palette;

public:
	AColorScheme(AColorPalette *palette = new RandomSaturatedPalette())
		: palette(palette)
	{};
	virtual ~AColorScheme();

	virtual AColorPattern* nextColor() =0;
	virtual void setPalette(AColorPalette* palette);
	virtual AColorPalette* getPalette();

};

#endif /* SRC_ANIMATION_COLORS_ABSTRACTCOLORSCHEME_H_ */
