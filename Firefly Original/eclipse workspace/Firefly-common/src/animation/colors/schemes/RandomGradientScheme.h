/*
 * RandomGradientScheme.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_SCHEMES_RANDOMGRADIENTSCHEME_H_
#define SRC_ANIMATION_COLORS_SCHEMES_RANDOMGRADIENTSCHEME_H_

#include <animation/base/AColorScheme.h>
#include <animation/colors/patterns/TwoColorGradientPattern.h>

class RandomGradientScheme: public AColorScheme {
public:
	RandomGradientScheme(AColorPalette *palette = new RandomSaturatedPalette())
		: AColorScheme(palette)
	{};
	virtual ~RandomGradientScheme();

	virtual AColorPattern* nextColor();
};

#endif /* SRC_ANIMATION_COLORS_SCHEMES_RANDOMGRADIENTSCHEME_H_ */
