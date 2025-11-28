SHELL := bash

ROOT := $(shell cd ..; pwd)

ALL := \
  javascript \

# ALL-BUILD := $(ALL:%=build-%)
ALL-TEST := $(ALL:%=test-%)
ALL-CLEAN := $(ALL:%=clean-%)

default:

# build: $(ALL-BUILD)

# build-%:
# 	$(MAKE) -C $(@:build-%=%) build

test: $(ALL-TEST)

test-%:
	$(MAKE) -C $(@:test-%=%) test TRACE=$(TRACE) DEBUG=$(DEBUG)

clean: $(ALL-CLEAN)
	$(MAKE) -C $(ROOT)/test $@

clean-%:
	$(MAKE) -C $(@:clean-%=%) clean
