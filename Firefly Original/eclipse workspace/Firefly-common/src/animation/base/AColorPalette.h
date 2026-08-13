/*
 * AColorPalette.h
 *
 *  Created on: Aug 24, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_ACOLORPALETTE_H_
#define SRC_ANIMATION_BASE_ACOLORPALETTE_H_

#include <animation/base/Timeable.h>
#include <glm/glm.hpp>

#define ACP_LAST_COLOR_MIN_DISTANCE 0.2

class AColorPalette : public Timeable {
protected:
	glm::vec4 lastColor;
	glm::vec4 checkAgainstLastColor(glm::vec4 color);

public:
	AColorPalette();
	virtual ~AColorPalette();

	virtual glm::vec4 nextColor() =0;

	virtual glm::vec4 randomColor() {
		if(numColors() == -1) {
			return nextColor();
		} else {
			for(int i = rand() % numColors(); i >= 0; i--) {
				nextColor();
			}
			return nextColor();
		}
	}

	// -1 if this palette will return random new colors infinitely
	// N if this is a palette of a static N colors. It is expected they repeat in order.
	virtual int numColors() {
		return -1;
	}

};

#endif /* SRC_ANIMATION_BASE_ACOLORPALETTE_H_ */
