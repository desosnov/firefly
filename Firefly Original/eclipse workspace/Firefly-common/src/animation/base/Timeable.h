/*
 * ATimeable.h
 *
 *  Created on: Aug 24, 2016
 *      Author: d
 */

#ifndef SRC_ANIMATION_BASE_TIMEABLE_H_
#define SRC_ANIMATION_BASE_TIMEABLE_H_

class Timeable {
public:
	Timeable();
	virtual ~Timeable();

	virtual void init(double time) {};
	virtual void update(double time) {};
};

#endif /* SRC_ANIMATION_BASE_TIMEABLE_H_ */
