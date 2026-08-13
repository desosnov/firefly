/*
 * CalibrationAnimation.h
 *
 *  Created on: Feb 9, 2016
 *      Author: d
 */

#ifndef SRC_CALIBRATIONANIMATION_H_
#define SRC_CALIBRATIONANIMATION_H_

#include <map>
#include "glm/glm.hpp"
#include "stage/PixelStage.h"

#define CALIBRATION_ANCHOR_MOVE_INTERVAL .01
#define CALIBRATION_DEFAULT_COLOR (glm::vec3(0.2, 0.2, 0.2))
#define CALIBRATION_SELECTED_COLOR (glm::vec3(1.0, 0.0, 0.0))
#define CALIBRATION_ANCHOR_COLOR (glm::vec3(0.0, 0.0, 1.0))

enum Phase {
	ANCHOR_SELECTION = 1,
	REFERENCE_SELECTION = 2,
	MOVEMENT = 3
};

class CylinderCalibration {
private:
	PixelStage *pixels;
	int anchor;
	int reference;
	Phase phase;

	std::map<int, double> anchors;

	double radialAtIndex(int index);
	int nearestIndexToRadial(double radial);

public:
	CylinderCalibration(PixelStage *pixelStage);
	virtual ~CylinderCalibration();

	void goLeft(int increment);
	void goRight(int increment);
	void select();
	void cancel();
	void printCalibration();

	Pixel pixelInFocus();

	void lightPixels(double time);
};

#endif /* SRC_CALIBRATIONANIMATION_H_ */
