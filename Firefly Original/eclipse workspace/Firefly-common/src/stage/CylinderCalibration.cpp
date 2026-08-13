/*
 * CalibrationAnimation.cpp
 *
 *  Created on: Feb 9, 2016
 *      Author: d
 */

#include "stage/CylinderCalibration.h"

#include <iostream>
#include "FireflyUtils.h"
#include "stage/PixelStage.h"

#define _USE_MATH_DEFINES
#include <math.h>

using namespace std;

CylinderCalibration::CylinderCalibration(PixelStage *pixelStage) {
	pixels = pixelStage;
	anchor = 0;
	reference = 0;
	phase = ANCHOR_SELECTION;
	anchors = pixels->getAnchors();
}

CylinderCalibration::~CylinderCalibration() {
	// TODO Auto-generated destructor stub
}

void CylinderCalibration::goLeft(int increment) {
	switch(phase) {
	case ANCHOR_SELECTION:
		if(anchor > increment)
			anchor -= increment;
		else
			anchor = 0;
		break;
	case REFERENCE_SELECTION:
		if(reference > increment)
			reference -= increment;
		else
			reference = 0;
		break;
	case MOVEMENT:
		for(std::map<int,double>::iterator iter = anchors.find(anchor); iter != anchors.end(); iter++) {
			anchors[iter->first] -= increment*CALIBRATION_ANCHOR_MOVE_INTERVAL;
		}
		pixels->setAnchors(anchors);
		break;
	}
}

void CylinderCalibration::goRight(int increment) {
	switch(phase) {
	case ANCHOR_SELECTION:
		anchor += increment;
		break;
	case REFERENCE_SELECTION:
		reference += increment;
		break;
	case MOVEMENT:
		for(std::map<int,double>::iterator iter = anchors.find(anchor); iter != anchors.end(); iter++) {
			anchors[iter->first] += increment*CALIBRATION_ANCHOR_MOVE_INTERVAL;
		}
		pixels->setAnchors(anchors);
		break;
	}
}

void CylinderCalibration::select() {
	double anchorRadial;

	switch(phase) {
	case ANCHOR_SELECTION:
		anchorRadial = radialAtIndex(anchor);
		anchors[anchor] = anchorRadial;
		pixels->setAnchors(anchors);
		reference = nearestIndexToRadial(anchorRadial - M_PI*2.0);
		phase = REFERENCE_SELECTION;
		break;
	case REFERENCE_SELECTION:
		phase = MOVEMENT;
		break;
	case MOVEMENT:
		anchor++;
		phase = ANCHOR_SELECTION;
		break;
	}
}

void CylinderCalibration::cancel() {
}

void CylinderCalibration::printCalibration() {
	for(map<int, double>::iterator iter = anchors.begin(); iter != anchors.end(); iter++) {
		cout << "anchors[" << iter->first << "] = " << iter->second << ";" << endl;
	}
}

double CylinderCalibration::radialAtIndex(int index) {
	int prevAnchor = 0, nextAnchor;
	double prevRadial = 0.0, nextRadial;
	for(std::map<int,double>::iterator iter = anchors.begin(); iter != anchors.end(); iter++) {
		nextAnchor = iter->first;
		nextRadial = iter->second;
		if(index >= prevAnchor && index <= nextAnchor)
			break;
		prevAnchor = nextAnchor;
		prevRadial = nextRadial;
	}
	return prevRadial + (nextRadial-prevRadial)/(nextAnchor-prevAnchor) * (index-prevAnchor);
}

int CylinderCalibration::nearestIndexToRadial(double radial) {
	if(radial < 0.0)
		return 0;

	int prevAnchor = 0, nextAnchor = 1;
	double prevRadial = 0.0, nextRadial = 1.0;
	for(std::map<int,double>::iterator iter = anchors.begin(); iter != anchors.end(); iter++) {
		nextAnchor = iter->first;
		nextRadial = iter->second;
		if(radial >= prevRadial && radial <= nextRadial)
			break;
		prevAnchor = nextAnchor;
		prevRadial = nextRadial;
	}

	return (int)round((double)prevAnchor + (double)(nextAnchor-prevAnchor)/(nextRadial-prevRadial) * (radial-prevRadial));
}

Pixel CylinderCalibration::pixelInFocus() {
	return pixels->pixels[anchor];
}

void CylinderCalibration::lightPixels(double time) {
	for(int pi = 0; pi < pixels->pixelsLen; pi++) {
		pixels->pixels[pi].setColor(CALIBRATION_DEFAULT_COLOR);
	}

	for(std::map<int,double>::iterator iter = anchors.begin(); iter != anchors.end(); iter++) {
		pixels->pixels[iter->first].setColor(CALIBRATION_ANCHOR_COLOR);
	}

	if(anchor >= pixels->pixelsLen) {
		anchor = pixels->pixelsLen-1;
	}
	if(reference >= pixels->pixelsLen) {
		reference = pixels->pixelsLen-1;
	}

	switch(phase) {
	case ANCHOR_SELECTION:
		pixels->pixels[anchor].setColor(CALIBRATION_SELECTED_COLOR);
		break;
	case REFERENCE_SELECTION:
		pixels->pixels[anchor].setColor(glm::vec3(1.0, 1.0, 1.0));
		pixels->pixels[reference].setColor(CALIBRATION_SELECTED_COLOR);
		break;
	case MOVEMENT:
		pixels->pixels[anchor].setColor(CALIBRATION_SELECTED_COLOR);
		pixels->pixels[reference].setColor(glm::vec3(1.0, 1.0, 1.0));
		break;
	}

}
