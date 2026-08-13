/*
 * SingleRandomHuePalette.h
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PALETTES_SINGLERANDOMHUEPALETTE_H_
#define SRC_ANIMATION_COLORS_PALETTES_SINGLERANDOMHUEPALETTE_H_

#include <animation/base/AColorPalette.h>

#define SRH_MIN_SATURATION 0.5
#define SRH_MAX_SATURATION 1.0

class SingleRandomHuePalette: public AColorPalette {
protected:
	float hue;

public:
	SingleRandomHuePalette();
	virtual ~SingleRandomHuePalette();

	virtual glm::vec4 nextColor();
	void randomizeHue();
};

#endif /* SRC_ANIMATION_COLORS_PALETTES_SINGLERANDOMHUEPALETTE_H_ */
