M := ../../.cache/makes
$(shell [ -d $M ] || git clone -q https://github.com/makeplus/makes $M)
include $M/init.mk
include $M/node.mk
include $M/clean.mk
include $M/shell.mk

ROOT := $(shell cd ../..; pwd)
BASE := $(ROOT)/parser-1.2
PARSER-LANG := coffeescript
BIN := coffee
GRAMMAR := lib/grammar.coffee

TEST-DEPS := \
  $(ROOT)/test/testml \
  $(ROOT)/test/suite \

include $(ROOT)/.common.mk
