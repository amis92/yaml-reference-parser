M := ../.cache/makes
$(shell [ -d $M ] || git clone -q https://github.com/makeplus/makes $M)
include $M/init.mk

ROOT := $(shell cd ..; pwd)

ALL := \
  coffeescript \
  csharp \
  javascript \
  perl \

ALL-BUILD := $(ALL:%=build-%)
ALL-TEST := $(ALL:%=test-%)
ALL-CLEAN := $(ALL:%=clean-%)

build: $(ALL-BUILD)

build-%:
	$(MAKE) -C $(@:build-%=%) build

test: $(ALL-TEST)

test-%:
	$(MAKE) -C $(@:test-%=%) test TRACE=$(TRACE) DEBUG=$(DEBUG)

clean: $(ALL-CLEAN)
	$(MAKE) -C $(ROOT)/test $@

clean-%:
	$(MAKE) -C $(@:clean-%=%) clean
