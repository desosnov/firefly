/*
 * CosineEase4D.h
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_EASING_COSINEEASE4D_H_
#define SRC_ANIMATION_EASING_COSINEEASE4D_H_

#include <animation/base/AEasingFunction4D.h>
#include "FireflyUtils.h"

class CosineEase4D: public AEasingFunction4D {
public:
	CosineEase4D(
			double start,
			double finish,
			glm::vec4 easeFrom,
			glm::vec4 easeTo) : AEasingFunction4D(start, finish, easeFrom, easeTo)
	{};
	virtual ~CosineEase4D();

	glm::vec4 easedValue(double input);
};

#endif /* SRC_ANIMATION_EASING_COSINEEASE4D_H_ */
