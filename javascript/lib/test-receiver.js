// Generated by CoffeeScript 2.5.1
(function() {
  var TestReceiver;

  require('./prelude');

  global.TestReceiver = TestReceiver = class TestReceiver {
    constructor() {
      this.event = [];
      this.cache = [];
    }

    add(type, value) {
      var event;
      event = {
        type: type
      };
      if (this.header != null) {
        event.header = this.header;
        delete this.header;
      }
      if (this.anchor != null) {
        event.anchor = this.anchor;
        delete this.anchor;
      }
      if (this.tag != null) {
        event.tag = this.tag;
        delete this.tag;
      }
      if (value != null) {
        event.value = value;
      }
      this.push(event);
      return event;
    }

    push(event) {
      if (this.cache.length) {
        return _.last(this.cache).push(event);
      } else {
        return this.send(event);
      }
    }

    cache_up(event = null) {
      this.cache.push([]);
      if (event != null) {
        return this.add(event);
      }
    }

    cache_down(event = null) {
      var e, events, i, len;
      events = this.cache.pop() || xxxxx(this);
      for (i = 0, len = events.length; i < len; i++) {
        e = events[i];
        this.push(e);
      }
      if (event != null) {
        return this.add(event);
      }
    }

    cache_drop() {
      var events;
      events = this.cache.pop() || xxxxx(this);
      return events[0];
    }

    send(event) {
      return this.event.push(event);
    }

    output() {
      var output;
      output = this.event.map(function(e) {
        return e.type + (e.header ? ` ${e.header}` : '') + (e.anchor ? ` ${e.anchor}` : '') + (e.tag ? ` <${e.tag}>` : '') + (e.value ? ` ${e.value}` : '') + "\n";
      });
      return output.join('');
    }

    try__l_yaml_stream() {
      return this.add('+STR');
    }

    got__l_yaml_stream() {
      return this.add('-STR');
    }

    try__l_bare_document() {
      return this.add('+DOC');
    }

    got__l_bare_document() {
      return this.add('-DOC');
    }

    got__c_flow_mapping__all__x7b() {
      return this.add('+MAP {}');
    }

    got__c_flow_mapping__all__x7d() {
      return this.add('-MAP');
    }

    got__c_flow_sequence__all__x5b() {
      return this.add('+SEQ []');
    }

    got__c_flow_sequence__all__x5d() {
      return this.add('-SEQ');
    }

    try__l_block_mapping() {
      return this.cache_up('+MAP');
    }

    got__l_block_mapping() {
      return this.cache_down('-MAP');
    }

    not__l_block_mapping() {
      return this.cache_drop();
    }

    try__l_block_sequence() {
      return this.cache_up('+SEQ');
    }

    got__l_block_sequence() {
      return this.cache_down('-SEQ');
    }

    not__l_block_sequence() {
      var event;
      event = this.cache_drop();
      this.anchor = event.anchor;
      return this.tag = event.tag;
    }

    try__ns_l_compact_mapping() {
      return this.cache_up('+MAP');
    }

    got__ns_l_compact_mapping() {
      return this.cache_down('-MAP');
    }

    not__ns_l_compact_mapping() {
      return this.cache_drop();
    }

    try__ns_flow_pair() {
      return this.cache_up();
    }

    got__ns_flow_pair() {
      return xxxxx(this);
    }

    not__ns_flow_pair() {
      return this.cache_drop();
    }

    try__ns_l_block_map_implicit_entry() {
      return this.cache_up();
    }

    got__ns_l_block_map_implicit_entry() {
      return this.cache_down();
    }

    not__ns_l_block_map_implicit_entry() {
      return this.cache_drop();
    }

    try__c_ns_flow_map_empty_key_entry() {
      return this.cache_up();
    }

    got__c_ns_flow_map_empty_key_entry() {
      return xxxxx(this);
    }

    not__c_ns_flow_map_empty_key_entry() {
      return this.cache_drop();
    }

    got__ns_plain(o) {
      return this.add('=VAL', ':' + o.text);
    }

    got__c_single_quoted(o) {
      return this.add('=VAL', "'" + o.text.slice(1, -1));
    }

    got__c_double_quoted(o) {
      return this.add('=VAL', '"' + o.text.slice(1, -1));
    }

    got__e_scalar() {
      return this.add('=VAL', ':');
    }

    got__c_directives_end(o) {
      return this.header = '---';
    }

    got__c_ns_anchor_property(o) {
      return this.anchor = o.text;
    }

    got__c_ns_tag_property(o) {
      return this.tag = o.text;
    }

    got__c_ns_alias_node(o) {
      return this.add('=ALI', o.text);
    }

  };

  // vim: sw=2:

}).call(this);