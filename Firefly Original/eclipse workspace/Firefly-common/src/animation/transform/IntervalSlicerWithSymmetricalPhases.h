/*
 * IntervalSlicerWithSymmetricalPhases.h
 *
 *  Created on: Aug 6, 2016
 *      Author: Denis
 */

#ifndef ANIMATION_TRANSFORM_INTERVALSLICERWITHSYMMETRICALPHASES_H_
#define ANIMATION_TRANSFORM_INTERVALSLICERWITHSYMMETRICALPHASES_H_

#include <math.h>

class IntervalSlicerWithSymmetricalPhases {
public:
	double center, interval;

	IntervalSlicerWithSymmetricalPhases(
			double center,
			double intervalSize
			) : center(center), interval(intervalSize)
	{};

	int getInterval(double point);
	double getPhase(double point);
	double getSymmetricalPhase(double point);
};

#endif /* ANIMATION_TRANSFORM_INTERVALSLICERWITHSYMMETRICALPHASES_H_ */
