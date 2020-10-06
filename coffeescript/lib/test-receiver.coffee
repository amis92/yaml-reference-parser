require './prelude'

global.TestReceiver = class TestReceiver
  constructor: ->
    @event = []
    @cache = []

  add: (type, value)->
    event = type: type
    if @header?
      event.header = @header
      delete @header
    if @anchor?
      event.anchor = @anchor
      delete @anchor
    if @tag?
      event.tag = @tag
      delete @tag
    if value?
      event.value = value
    @push event
    return event

  push: (event)->
    if @cache.length
      _.last(@cache).push event
    else
      @send event

  cache_up: (event=null)->
    @cache.push []
    @add event if event?

  cache_down: (event=null)->
    events = @cache.pop() or xxxxx @
    @push e for e in events
    @add event if event?

  cache_drop: ->
    events = @cache.pop() or xxxxx @
    return events[0]

  send: (event)->
    @event.push event

  output: ->
    output = @event.map (e)->
      e.type +
        (if e.header then " #{e.header}" else '') +
        (if e.anchor then " #{e.anchor}" else '') +
        (if e.tag then " <#{e.tag}>" else '') +
        (if e.value then " #{e.value}" else '') +
        "\n"
    output.join ''

  try__l_yaml_stream: -> @add '+STR'
  got__l_yaml_stream: -> @add '-STR'

  try__l_bare_document: -> @add '+DOC'
  got__l_bare_document: -> @add '-DOC'

  got__c_flow_mapping__all__x7b: -> @add '+MAP {}'
  got__c_flow_mapping__all__x7d: -> @add '-MAP'

  got__c_flow_sequence__all__x5b: -> @add '+SEQ []'
  got__c_flow_sequence__all__x5d: -> @add '-SEQ'

  try__l_block_mapping: -> @cache_up '+MAP'
  got__l_block_mapping: -> @cache_down '-MAP'
  not__l_block_mapping: -> @cache_drop()

  try__l_block_sequence: -> @cache_up '+SEQ'
  got__l_block_sequence: -> @cache_down '-SEQ'
  not__l_block_sequence: ->
    event = @cache_drop()
    @anchor = event.anchor
    @tag = event.tag

  try__ns_l_compact_mapping: -> @cache_up '+MAP'
  got__ns_l_compact_mapping: -> @cache_down '-MAP'
  not__ns_l_compact_mapping: -> @cache_drop()

  try__ns_flow_pair: -> @cache_up()
  got__ns_flow_pair: -> xxxxx @
  not__ns_flow_pair: -> @cache_drop()

  try__ns_l_block_map_implicit_entry: -> @cache_up()
  got__ns_l_block_map_implicit_entry: -> @cache_down()
  not__ns_l_block_map_implicit_entry: -> @cache_drop()

  try__c_ns_flow_map_empty_key_entry: -> @cache_up()
  got__c_ns_flow_map_empty_key_entry: -> xxxxx @
  not__c_ns_flow_map_empty_key_entry: -> @cache_drop()

  got__ns_plain: (o)-> @add '=VAL', ':' + o.text
  got__c_single_quoted: (o)->
    @add '=VAL', "'" + o.text[1...-1]
  got__c_double_quoted: (o)->
    @add '=VAL', '"' + o.text[1...-1]
  got__e_scalar: -> @add '=VAL', ':'

  got__c_directives_end: (o)-> @header = '---'

  got__c_ns_anchor_property: (o)-> @anchor = o.text

  got__c_ns_tag_property: (o)-> @tag = o.text

  got__c_ns_alias_node: (o)-> @add '=ALI', o.text

# vim: sw=2: