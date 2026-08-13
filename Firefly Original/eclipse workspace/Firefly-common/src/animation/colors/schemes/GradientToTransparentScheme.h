/*
 * GradientToTransparentScheme.h
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOTRANSPARENTSCHEME_H_
#define SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOTRANSPARENTSCHEME_H_

#include <animation/base/AColorScheme.h>

class GradientToTransparentScheme : public AColorScheme {
public:
	GradientToTransparentScheme(AColorPalette *palette = new RandomSaturatedPalette())
		: AColorScheme(palette)
	{};
	virtual ~GradientToTransparentScheme();

	virtual AColorPattern* nextColor();
};


#endif /* SRC_ANIMATION_COLORS_SCHEMES_GRADIENTTOTRANSPARENTSCHEME_H_ */
