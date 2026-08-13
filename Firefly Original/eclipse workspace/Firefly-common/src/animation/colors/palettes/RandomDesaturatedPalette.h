/*
 * RandomDesaturatedPalette.h
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PALETTES_RANDOMDESATURATEDPALETTE_H_
#define SRC_ANIMATION_COLORS_PALETTES_RANDOMDESATURATEDPALETTE_H_

#include <animation/base/AColorPalette.h>

#define RDP_DEFAULT_MAX_SATURATION 0.75
#define RDP_MIN_SATURATION 0.5
#define RDP_MAX_SATURATION 0.9

class RandomDesaturatedPalette: public AColorPalette {
private:
	double maxSaturation;

public:
	RandomDesaturatedPalette(double maxSaturation = RDP_DEFAULT_MAX_SATURATION)
		: maxSaturation(maxSaturation)
	{};
	virtual ~RandomDesaturatedPalette();

	void setMaxSaturation(double sat);
	glm::vec4 nextColor();
};

#endif /* SRC_ANIMATION_COLORS_PALETTES_RANDOMDESATURATEDPALETTE_H_ */
