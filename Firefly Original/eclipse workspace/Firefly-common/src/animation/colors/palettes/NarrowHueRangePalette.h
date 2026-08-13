/*
 * RandomHueRangePalette.h
 *
 *  Created on: Aug 30, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PALETTES_NARROWHUERANGEPALETTE_H_
#define SRC_ANIMATION_COLORS_PALETTES_NARROWHUERANGEPALETTE_H_

#include <animation/base/AColorPalette.h>

#define NHR_MIN_SATURATION 0.5
#define NHR_MAX_SATURATION 1.0
#define NHR_MIN_HUE_RANGE 20.0
#define NHR_MAX_HUE_RANGE 100.0

class NarrowHueRangePalette: public AColorPalette {
protected:
	float minHue, maxHue;

public:
	NarrowHueRangePalette();
	virtual ~NarrowHueRangePalette();

	virtual glm::vec4 nextColor();
	void randomizeHue();
};

#endif /* SRC_ANIMATION_COLORS_PALETTES_NARROWHUERANGEPALETTE_H_ */
