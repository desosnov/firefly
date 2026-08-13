/*
 * SolidColorPattern.h
 *
 *  Created on: Aug 25, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_COLORS_PATTERNS_SOLIDPATTERN_H_
#define SRC_ANIMATION_COLORS_PATTERNS_SOLIDPATTERN_H_

#include <animation/base/AColorPattern.h>

class SolidPattern: public AColorPattern {
public:
	SolidPattern(glm::vec4 color = glm::vec4(1.0, 1.0, 1.0, 1.0))
		: AColorPattern(color)
	{};
	virtual ~SolidPattern();

	glm::vec4 getColor();
};

#endif /* SRC_ANIMATION_COLORS_PATTERNS_SOLIDPATTERN_H_ */
