/*
 * TwoRandomHuesPalette.h
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PALETTES_TWORANDOMHUESPALETTE_H_
#define SRC_ANIMATION_COLORS_PALETTES_TWORANDOMHUESPALETTE_H_

#include <animation/base/AColorPalette.h>

#define TRH_MIN_HUE_DISTANCE 36.0
#define TRH_MIN_SATURATION 0.5
#define TRH_MAX_SATURATION 1.0

class TwoRandomHuesPalette: public AColorPalette {
protected:
	float hue1, hue2;

public:
	TwoRandomHuesPalette();
	virtual ~TwoRandomHuesPalette();

	virtual glm::vec4 nextColor();
	void randomizeHues();
};

#endif /* SRC_ANIMATION_COLORS_PALETTES_TWORANDOMHUESPALETTE_H_ */
