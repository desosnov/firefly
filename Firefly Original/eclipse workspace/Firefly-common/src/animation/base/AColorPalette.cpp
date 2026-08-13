/*
 * AColorPalette.cpp
 *
 *  Created on: Aug 24, 2016
 *      Author: d
 */

#include <animation/base/AColorPalette.h>

AColorPalette::AColorPalette() {
	// TODO Auto-generated constructor stub

}

AColorPalette::~AColorPalette() {
	// TODO Auto-generated destructor stub
}

glm::vec4 AColorPalette::checkAgainstLastColor(glm::vec4 color) {
	if(glm::distance(color, lastColor) < ACP_LAST_COLOR_MIN_DISTANCE) {
		return nextColor();
	} else {
		lastColor = color;
		return color;
	}
}
