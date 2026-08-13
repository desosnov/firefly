/*
 * TwoColorGradientPattern.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PATTERNS_TWOCOLORGRADIENTPATTERN_H_
#define SRC_ANIMATION_COLORS_PATTERNS_TWOCOLORGRADIENTPATTERN_H_

#include <animation/base/AColorPattern.h>

class TwoColorGradientPattern: public AColorPattern {
public:
	glm::vec4 color2;

	TwoColorGradientPattern(
			glm::vec4 color = glm::vec4(1.0, 1.0, 1.0, 1.0),
			glm::vec4 color2 = glm::vec4(0.0, 0.0, 0.0, 1.0))
			: AColorPattern(color), color2(color2)
	{};
	virtual ~TwoColorGradientPattern();

	virtual glm::vec4 getColor();
	virtual glm::vec4 getColor(double x);
};

#endif /* SRC_ANIMATION_COLORS_PATTERNS_TWOCOLORGRADIENTPATTERN_H_ */
