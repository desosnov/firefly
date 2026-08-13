/*
 * EasingFunction1D.h
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_EASING_EASINGFUNCTION1D_H_
#define SRC_ANIMATION_EASING_EASINGFUNCTION1D_H_

#include <vector>
#include <algorithm>
#include <math.h>

class AEasingFunction1D {
protected:
	std::vector<double*> bindings;
	bool finishedFlag = false;

	double normalizeInput(double input);

public:
	double start, finish;
	double easeFrom, easeTo;

	AEasingFunction1D(
			double start,
			double finish,
			double easeFrom,
			double easeTo)
			: start(start),
			  finish(finish),
			  easeFrom(easeFrom),
			  easeTo(easeTo) {};
	virtual ~AEasingFunction1D();

	void bindValue(double* valPtr);
	void update(double time);
	bool finished();

	virtual double easedValue(double input) =0;
};

#endif /* SRC_ANIMATION_EASING_EASINGFUNCTION1D_H_ */
