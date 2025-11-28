SHELL := bash

.PHONY: test

ifeq (,$(ROOT))
  $(error ROOT not defined)
endif
ifeq (,$(BASE))
  $(error BASE not defined)
endif
BASE12 := $(ROOT)/parser-1.2

LOCAL-MAKE := $(ROOT)/.git/local.mk
ifneq (,$(wildcard $(LOCAL-MAKE)))
  $(info ***** USING LOCAL MAKEFILE OVERRIDES *****)
  $(info ***** $(LOCAL-MAKE))
  include $(LOCAL-MAKE)
endif

SPEC-PATCHED-YAML := $(BASE)/build/yaml-spec-1.2-patched.yaml
SPEC-YAML := $(BASE)/build/yaml-spec-1.2.yaml
SPEC-PATCH := $(BASE)/build/yaml-spec-1.2.patch
GENERATOR := $(BASE)/build/bin/generate-yaml-grammar
GENERATOR-LIB := $(BASE)/build/lib/generate-yaml-grammar.coffee
GENERATOR-LANG-LIB := $(BASE)/build/lib/generate-yaml-grammar-$(PARSER-LANG).coffee
NODE-MODULES := $(ROOT)/node_modules

TESTML_REPO ?= https://github.com/testml-lang/testml
TESTML_COMMIT ?= master
YAML-TEST-SUITE-REPO ?= https://github.com/yaml/yaml-test-suite
YAML-TEST-SUITE-COMMIT ?= main

override PATH := $(NODE-MODULES)/.bin:$(PATH)
override PATH := $(ROOT)/test/testml/bin:$(PATH)
export PATH

export TESTML_RUN := $(BIN)-tap
export TESTML_LIB := $(ROOT)/test:$(ROOT)/test/suite/test:$(TESTML_LIB)

BUILD-DEPS ?= $(NODE-MODULES)
TEST-DEPS ?= \
  $(ROOT)/test/testml \
  $(ROOT)/test/suite \

test := test/*.tml

.DELETE_ON_ERROR:

default::

build:: $(BUILD-DEPS) $(GRAMMAR)

test:: build $(TEST-DEPS)
	TRACE=$(TRACE) TRACE_QUIET=$(TRACE_QUIET) DEBUG=$(DEBUG) \
	  prove -v $(test)

clean::

$(GRAMMAR): $(SPEC-PATCHED-YAML) $(GENERATOR) $(GENERATOR-LIB) $(GENERATOR-LANG-LIB)
	$(GENERATOR) \
	  --from=$< \
	  --to=$(PARSER-LANG) \
	  --rule=l-yaml-stream \
	> $@

$(SPEC-PATCHED-YAML): $(SPEC-YAML)
	cp $< $@
	patch $@ < $(SPEC-PATCH)

$(SPEC-YAML):
	cp $(ROOT)/../yaml-grammar/yaml-spec-1.2.yaml $@ || \
	wget https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml || \
	curl -O https://raw.githubusercontent.com/yaml/yaml-grammar/master/yaml-spec-1.2.yaml

$(ROOT)/test/suite \
$(ROOT)/test/testml: $(ROOT)/test $(BASE12)/perl/ext-perl
	$(eval override export PERL5LIB := $(BASE12)/perl/ext-perl/lib/perl5:$(PERL5LIB))
	$(MAKE) -C $< all

$(NODE-MODULES): $(NODE)
	$(MAKE) -C $(ROOT) $(@:$(ROOT)/%=%)

$(BASE12)/perl/ext-perl:
	$(MAKE) -C $(BASE12)/perl ext-perl


define git-clone
mkdir $1
git -C $1 init --quiet
git -C $1 remote add origin $2
git -C $1 fetch origin $3
git -C $1 reset --hard FETCH_HEAD
endef
