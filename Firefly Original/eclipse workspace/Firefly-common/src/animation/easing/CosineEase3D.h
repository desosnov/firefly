/*
 * CosineEase3D.h
 *
 *  Created on: Aug 8, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_EASING_COSINEEASE3D_H_
#define ANIMATION_EASING_COSINEEASE3D_H_

#include <animation/base/AEasingFunction3D.h>
#include "FireflyUtils.h"

class CosineEase3D: public AEasingFunction3D {
public:
	CosineEase3D(
			double start,
			double finish,
			glm::vec3 easeFrom,
			glm::vec3 easeTo) : AEasingFunction3D(start, finish, easeFrom, easeTo)
	{};
	virtual ~CosineEase3D();

	glm::vec3 easedValue(double input);
};

#endif /* ANIMATION_EASING_COSINEEASE3D_H_ */
