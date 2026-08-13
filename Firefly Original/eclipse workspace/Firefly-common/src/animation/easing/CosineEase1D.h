/*
 * CosineEase1D.h
 *
 *  Created on: Aug 8, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_EASING_COSINEEASE1D_H_
#define ANIMATION_EASING_COSINEEASE1D_H_

#include <animation/base/AEasingFunction1D.h>

class CosineEase1D: public AEasingFunction1D {
public:
	CosineEase1D(
			double start,
			double finish,
			double easeFrom,
			double easeTo) : AEasingFunction1D(start, finish, easeFrom, easeTo)
	{};
	virtual ~CosineEase1D();

	double easedValue(double input);
};

#endif /* ANIMATION_EASING_COSINEEASE1D_H_ */
