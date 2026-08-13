/*
 * AColorPattern.h
 *
 *  Created on: Aug 24, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_ACOLORPATTERN_H_
#define SRC_ANIMATION_BASE_ACOLORPATTERN_H_

#include <animation/base/Timeable.h>
#include <glm/glm.hpp>

class AColorPattern : public Timeable {
public:
	glm::vec4 color;

	AColorPattern(glm::vec4 color = glm::vec4(1.0, 1.0, 1.0, 1.0))
		: color(color)
	{};
	virtual ~AColorPattern();

	virtual glm::vec4 getColor() =0;
	virtual glm::vec4 getColor(double x); // 0 to 1
	virtual glm::vec4 getColor(double x, double y);
	virtual glm::vec4 getColor(glm::vec3 pos);
	virtual glm::vec4 getColor(double x, glm::vec3 pos);
	virtual glm::vec4 getColor(double x, double y, glm::vec3 pos);

};

#endif /* SRC_ANIMATION_BASE_ACOLORPATTERN_H_ */
