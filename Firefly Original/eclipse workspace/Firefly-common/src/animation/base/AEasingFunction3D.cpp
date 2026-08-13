/*
 * EasingFunction3D.cpp
 *
 *  Created on: Mar 11, 2016
 *      Author: d
 */

#include <animation/base/AEasingFunction3D.h>

AEasingFunction3D::~AEasingFunction3D() {
	// TODO Auto-generated destructor stub
}

void AEasingFunction3D::bindValue(glm::vec3* valPtr) {
	if(std::find(bindings.begin(), bindings.end(), valPtr) == bindings.end()) {
		bindings.push_back(valPtr);
	}
}

void AEasingFunction3D::update(double time) {
	for(std::vector<glm::vec3*>::iterator iter = bindings.begin(); iter != bindings.end(); iter++) {
		*(*iter) = easedValue(time);
	}

	if((time - start) / (finish - start) > 1.0) {
		finishedFlag = true;
	} else {
		finishedFlag = false;
	}
}

double AEasingFunction3D::normalizeInput(double input) {
	double normalized = (input - start) / (finish - start);

	return fmax(fmin(normalized, 1.0), 0.0);
}


bool AEasingFunction3D::finished() {
	return finishedFlag;
}
