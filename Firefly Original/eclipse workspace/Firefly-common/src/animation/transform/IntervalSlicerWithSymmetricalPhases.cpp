/*
 * IntervalSlicerWithSymmetricalPhases.cpp
 *
 *  Created on: Aug 6, 2016
 *      Author: Denis
 */

#include <animation/transform/IntervalSlicerWithSymmetricalPhases.h>

int IntervalSlicerWithSymmetricalPhases::getInterval(double point) {
	return (int)floor((point-center)/interval);
}

double IntervalSlicerWithSymmetricalPhases::getPhase(double point) {
	return fmod(point-center, interval)/interval; // Normalize to a value from 0 to 1 across the interval
}

double IntervalSlicerWithSymmetricalPhases::getSymmetricalPhase(double point) {
	double phase = getPhase(point);
	phase -= 0.5; // Shift to (-0.5, 0.5)
	phase *= 2; // Shift to (-1.0, 1.0)
	phase = 1.0 - fabs(phase); // Take the absolute value and invert so center of interval is 1.0
	return phase;
}
