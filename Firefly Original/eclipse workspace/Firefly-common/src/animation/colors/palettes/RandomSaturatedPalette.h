/*
 * RandomChromaticColors.h
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_RANDOMCHROMATICCOLORS_H_
#define SRC_ANIMATION_COLORS_RANDOMCHROMATICCOLORS_H_

#include <animation/base/AColorPalette.h>
#include <animation/colors/HSVRGB.h>
#include <FireflyUtils.h>

#define RSP_DEFAULT_SATURATION 0.75
#define RSP_MIN_SATURATION 0.0
#define RSP_MAX_SATURATION 0.9


class RandomSaturatedPalette: public AColorPalette {
private:
	double saturation;

public:
	RandomSaturatedPalette(double minSaturation = RSP_DEFAULT_SATURATION)
		: saturation(minSaturation)
	{};
	virtual ~RandomSaturatedPalette();

	void setMinSaturation(double sat);
	glm::vec4 nextColor();
};

#endif /* SRC_ANIMATION_COLORS_RANDOMCHROMATICCOLORS_H_ */
