/*
 * AEasingFunction4D.cpp
 *
 *  Created on: Aug 29, 2016
 *      Author: d
 */

#include <animation/base/AEasingFunction4D.h>

AEasingFunction4D::~AEasingFunction4D() {
	// TODO Auto-generated destructor stub
}

void AEasingFunction4D::bindValue(glm::vec4* valPtr) {
	if(std::find(bindings.begin(), bindings.end(), valPtr) == bindings.end()) {
		bindings.push_back(valPtr);
	}
}

void AEasingFunction4D::update(double time) {
	for(std::vector<glm::vec4*>::iterator iter = bindings.begin(); iter != bindings.end(); iter++) {
		*(*iter) = easedValue(time);
	}

	if((time - start) / (finish - start) > 1.0) {
		finishedFlag = true;
	} else {
		finishedFlag = false;
	}
}

double AEasingFunction4D::normalizeInput(double input) {
	double normalized = (input - start) / (finish - start);

	return fmax(fmin(normalized, 1.0), 0.0);
}


bool AEasingFunction4D::finished() {
	return finishedFlag;
}
