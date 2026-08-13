/*
 * WideHueRangePalette.h
 *
 *  Created on: Sep 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PALETTES_WIDEHUERANGEPALETTE_H_
#define SRC_ANIMATION_COLORS_PALETTES_WIDEHUERANGEPALETTE_H_

#include <animation/base/AColorPalette.h>

#define WHR_MIN_SATURATION 0.5
#define WHR_MAX_SATURATION 1.0
#define WHR_MIN_HUE_RANGE 100.0
#define WHR_MAX_HUE_RANGE 200.0

class WideHueRangePalette: public AColorPalette {
protected:
	float minHue, maxHue;

public:
	WideHueRangePalette();
	virtual ~WideHueRangePalette();

	virtual glm::vec4 nextColor();
	void randomizeHue();
};

#endif /* SRC_ANIMATION_COLORS_PALETTES_WIDEHUERANGEPALETTE_H_ */
