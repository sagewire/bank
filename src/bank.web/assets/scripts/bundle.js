/*!
 * jQuery Typeahead
 * Copyright (C) 2016 RunningCoder.org
 * Licensed under the MIT license
 *
 * @author Tom Bertrand
 * @version 2.7.6 (2016-12-23)
 * @link http://www.runningcoder.org/jquerytypeahead/
 */
;(function (factory) {
    if (typeof define === 'function' && define.amd) {
        define('jquery-typeahead', ['jquery'], function (jQuery) {
            return factory(jQuery);
        });
    } else if (typeof module === 'object' && module.exports) {
        module.exports = function (jQuery, root) {
            if (jQuery === undefined) {
                if (typeof window !== 'undefined') {
                    jQuery = require('jquery');
                }
                else {
                    jQuery = require('jquery')(root);
                }
            }
            return factory(jQuery);
        }();
    } else {
        factory(jQuery);
    }
}(function ($) {

    "use strict";

    window.Typeahead = {
        version: '2.7.6'
    };

    /**
     * @private
     * Default options
     *
     * @link http://www.runningcoder.org/jquerytypeahead/documentation/
     */
    var _options = {
        input: null,
        minLength: 2,           // Modified feature, now accepts 0 to search on focus
        maxItem: 8,             // Modified feature, now accepts 0 as "Infinity" meaning all the results will be displayed
        dynamic: false,
        delay: 300,
        order: null,            // ONLY sorts the first "display" key
        offset: false,
        hint: false,            // -> Improved feature, Added support for excessive "space" characters
        accent: false,          // -> Improved feature, define a custom replacement object
        highlight: true,        // -> Added "any" to highlight any word in the template, by default true will only highlight display keys
        group: false,           // -> Improved feature, Boolean,string,object(key, template (string, function))
        groupOrder: null,       // -> New feature, order groups "asc", "desc", Array, Function
        maxItemPerGroup: null,  // -> Renamed option
        dropdownFilter: false,  // -> Renamed option, true will take group options string will filter on object key
        dynamicFilter: null,    // -> New feature, filter the typeahead results based on dynamic value, Ex: Players based on TeamID
        backdrop: false,
        backdropOnFocus: false, // -> New feature, display the backdrop option as the Typeahead input is :focused
        cache: false,           // -> Improved option, true OR 'localStorage' OR 'sessionStorage'
        ttl: 3600000,
        compression: false,     // -> Requires LZString library
        suggestion: false,      // -> *Coming soon* New feature, save last searches and display suggestion on matched characters
        searchOnFocus: false,   // -> New feature, display search results on input focus
        resultContainer: null,  // -> New feature, list the results inside any container string or jQuery object
        generateOnLoad: null,   // -> New feature, forces the source to be generated on page load even if the input is not focused!
        mustSelectItem: false,  // -> New option, the submit function only gets called if an item is selected
        href: null,             // -> New feature, String or Function to format the url for right-click & open in new tab on link results
        display: ["display"],   // -> Improved feature, allows search in multiple item keys ["display1", "display2"]
        template: null,
        groupTemplate: null,    // -> New feature, set a custom template for the groups
        correlativeTemplate: false, // -> New feature, compile display keys, enables multiple key search from the template string
        emptyTemplate: false,   // -> New feature, display an empty template if no result
        cancelButton: true,     // -> New feature, if text is detected in the input, a cancel button will be available to reset the input (pressing ESC also cancels)
        loadingAnimation: true, // -> New feature, will display a loading animation when typeahead is doing request / searching for results
        filter: true,           // -> New feature, set to false or function to bypass Typeahead filtering. WARNING: accent, correlativeTemplate, offset & matcher will not be interpreted
        matcher: null,          // -> New feature, add an extra filtering function after the typeahead functions
        source: null,
        callback: {
            onInit: null,
            onReady: null,              // -> New callback, when the Typeahead initial preparation is completed
            onShowLayout: null,         // -> New callback, called when the layout is shown
            onHideLayout: null,         // -> New callback, called when the layout is hidden
            onSearch: null,             // -> New callback, when data is being fetched & analyzed to give search results
            onResult: null,
            onLayoutBuiltBefore: null,  // -> New callback, when the result HTML is build, modify it before it get showed
            onLayoutBuiltAfter: null,   // -> New callback, modify the dom right after the results gets inserted in the result container
            onNavigateBefore: null,     // -> New callback, when a key is pressed to navigate the results
            onNavigateAfter: null,      // -> New callback, when a key is pressed to navigate the results
            onMouseEnter: null,
            onMouseLeave: null,
            onClickBefore: null,        // -> Improved feature, possibility to e.preventDefault() to prevent the Typeahead behaviors
            onClickAfter: null,         // -> New feature, happens after the default clicked behaviors has been executed
            onDropdownFilter: null,     // -> New feature, when the dropdownFilter is changed, trigger this callback
            onSendRequest: null,        // -> New callback, gets called when the Ajax request(s) are sent
            onReceiveRequest: null,     // -> New callback, gets called when the Ajax request(s) are all received
            onPopulateSource: null,     // -> New callback, Perform operation on the source data before it gets in Typeahead data
            onCacheSave: null,          // -> New callback, Perform operation on the source data before it gets in Typeahead cache
            onSubmit: null,
            onCancel: null              // -> New callback, triggered if the typeahead had text inside and is cleared
        },
        selector: {
            container: "typeahead__container",
            result: "typeahead__result",
            list: "typeahead__list",
            group: "typeahead__group",
            item: "typeahead__item",
            empty: "typeahead__empty",
            display: "typeahead__display",
            query: "typeahead__query",
            filter: "typeahead__filter",
            filterButton: "typeahead__filter-button",
            dropdown: "typeahead__dropdown",
            dropdownItem: "typeahead__dropdown-item",
            button: "typeahead__button",
            backdrop: "typeahead__backdrop",
            hint: "typeahead__hint",
            cancelButton: "typeahead__cancel-button"
        },
        debug: false
    };

    /**
     * @private
     * Event namespace
     */
    var _namespace = ".typeahead";

    /**
     * @private
     * Accent equivalents
     */
    var _accent = {
        from: "ãàáäâẽèéëêìíïîõòóöôùúüûñç",
        to: "aaaaaeeeeeiiiiooooouuuunc"
    };

    /**
     * #62 IE9 doesn't trigger "input" event when text gets removed (backspace, ctrl+x, etc)
     * @private
     */
    var _isIE9 = ~window.navigator.appVersion.indexOf("MSIE 9.");

    /**
     * #193 Clicking on a suggested option does not select it on IE10/11
     * @private
     */
    var _isIE10 = ~window.navigator.appVersion.indexOf("MSIE 10");
    var _isIE11 = ~window.navigator.userAgent.indexOf("Trident") && ~window.navigator.userAgent.indexOf("rv:11");

    // SOURCE GROUP RESERVED WORDS: ajax, data, url
    // SOURCE ITEMS RESERVED KEYS: group, display, data, matchedKey, compiled, href

    /**
     * @constructor
     * Typeahead Class
     *
     * @param {object} node jQuery input object
     * @param {object} options User defined options
     */
    var Typeahead = function (node, options) {

        this.rawQuery = node.val() || '';   // Unmodified input query
        this.query = node.val() || '';      // Input query
        this.namespace = '.' + _namespace; // Every Typeahead instance gets its own namespace for events
        this.tmpSource = {};                // Temp var to preserve the source order for the searchResult function
        this.source = {};                   // The generated source kept in memory
        this.isGenerated = null;            // Generated results -> null: not generated, false: generating, true generated
        this.generatedGroupCount = 0;       // Number of groups generated, if limit reached the search can be done
        this.groupCount = 0;                // Number of groups, this value gets counted on the initial source unification
        this.groupBy = "group";             // This option will change according to filtering or custom grouping
        this.groups = [];                   // Array of all the available groups, used to build the groupTemplate
        this.result = {};                   // Results based on Source-query match (only contains the displayed elements)
        this.groupTemplate = '';            // Result template at the {{group}} level
        this.resultHtml = null;             // HTML Results (displayed elements)
        this.resultCount = 0;               // Total results based on Source-query match
        this.resultCountPerGroup = {};      // Total results based on Source-query match per group
        this.options = options;             // Typeahead options (Merged default & user defined)
        this.node = node;                   // jQuery object of the Typeahead <input>
        this.namespace = '.' +              // Every Typeahead instance gets its own namespace for events
            this.helper.slugify.call(this, node.selector) +
            _namespace;
        this.container = null;              // Typeahead container, usually right after <form>
        this.resultContainer = null;        // Typeahead result container (html)
        this.item = null;                   // The selected item
        this.xhr = {};                      // Ajax request(s) stack
        this.hintIndex = null;              // Numeric value of the hint index in the result list
        this.filters = {                    // Filter list for searching, dropdown and dynamic(s)
            dropdown: {},                   // Dropdown menu if options.dropdownFilter is set
            dynamic: {}                     // Checkbox / Radio / Select to filter the source data
        };
        this.dropdownFilter = {
            static: [],                     // Objects that has a value
            dynamic: []
        };
        this.dropdownFilterAll = null;      // The last "all" definition
        this.isDropdownEvent = false;       // If a dropdownFilter is clicked, this will be true to trigger the callback

        this.requests = {};                 // Store the group:request instead of generating them every time

        this.backdrop = {};                 // The backdrop object
        this.hint = {};                     // The hint object
        this.hasDragged = false;            // Will cancel mouseend events if true
        this.focusOnly = false;             // Focus the input preventing any operations

        this.__construct();

    };

    Typeahead.prototype = {

        extendOptions: function () {

            // If the Typeahead is dynamic, force no cache & no compression
            if (this.options.dynamic) {
                this.options.cache = false;
                this.options.compression = false;
            }

            var scope = this;

            if (this.options.cache) {
                this.options.cache = (function (cache) {

                    var supportedCache = ['localStorage', 'sessionStorage'],
                        supported;

                    if (cache === true) {
                        cache = 'localStorage';
                    } else if (typeof cache === "string" && !~supportedCache.indexOf(cache)) {
                        // {debug}
                        if (scope.options.debug) {
                            _debug.log({
                                'node': scope.node.selector,
                                'function': 'extendOptions()',
                                'message': 'Invalid options.cache, possible options are "localStorage" or "sessionStorage"'
                            });

                            _debug.print();
                        }
                        // {/debug}
                        return false;
                    }

                    supported = typeof window[cache] !== "undefined";

                    try {
                        window[cache].setItem("typeahead", "typeahead");
                        window[cache].removeItem("typeahead");
                    } catch (e) {
                        supported = false;
                    }

                    return supported && cache || false;
                }).call(this, this.options.cache);
            }

            if (this.options.compression) {
                if (typeof LZString !== 'object' || !this.options.cache) {
                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'extendOptions()',
                            'message': 'Missing LZString Library or options.cache, no compression will occur.'
                        });

                        _debug.print();
                    }
                    // {/debug}
                    this.options.compression = false;
                }
            }

            if (typeof this.options.maxItem !== "undefined" && (!/^\d+$/.test(this.options.maxItem) || this.options.maxItem === 0)) {
                this.options.maxItem = Infinity;
            }

            if (this.options.maxItemPerGroup && !/^\d+$/.test(this.options.maxItemPerGroup)) {
                this.options.maxItemPerGroup = null;
            }

            if (this.options.display && !Array.isArray(this.options.display)) {
                this.options.display = [this.options.display];
            }

            if (this.options.group) {
                if (!Array.isArray(this.options.group)) {
                    if (typeof this.options.group === "string") {
                        this.options.group = {
                            key: this.options.group
                        };
                    } else if (typeof this.options.group === "boolean") {
                        this.options.group = {
                            key: 'group'
                        };
                    }

                    this.options.group.key = this.options.group.key || "group";
                }
                // {debug}
                else {
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'extendOptions()',
                            'message': 'options.group must be a boolean|string|object as of 2.5.0'
                        });

                        _debug.print();
                    }
                }
                // {/debug}
            }

            if (this.options.highlight && !~["any", true].indexOf(this.options.highlight)) {
                this.options.highlight = false;
            }

            if (this.options.dropdownFilter && this.options.dropdownFilter instanceof Object) {
                if (!Array.isArray(this.options.dropdownFilter)) {
                    this.options.dropdownFilter = [this.options.dropdownFilter];
                }
                for (var i = 0, ii = this.options.dropdownFilter.length; i < ii; ++i) {
                    this.dropdownFilter[this.options.dropdownFilter[i].value ? 'static' : 'dynamic'].push(this.options.dropdownFilter[i]);
                }
            }

            if (this.options.dynamicFilter && !Array.isArray(this.options.dynamicFilter)) {
                this.options.dynamicFilter = [this.options.dynamicFilter];
            }

            if (this.options.accent) {
                if (typeof this.options.accent === "object") {
                    if (this.options.accent.from && this.options.accent.to && this.options.accent.from.length === this.options.accent.to.length) {

                    }
                    // {debug}
                    else {
                        if (this.options.debug) {
                            _debug.log({
                                'node': this.node.selector,
                                'function': 'extendOptions()',
                                'message': 'Invalid "options.accent", from and to must be defined and same length.'
                            });

                            _debug.print();
                        }
                    }
                    // {/debug}
                } else {
                    this.options.accent = _accent;
                }
            }

            if (this.options.groupTemplate) {
                this.groupTemplate = this.options.groupTemplate;
            }

            if (this.options.resultContainer) {
                if (typeof this.options.resultContainer === "string") {
                    this.options.resultContainer = $(this.options.resultContainer);
                }

                if (!(this.options.resultContainer instanceof $) || !this.options.resultContainer[0]) {
                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'extendOptions()',
                            'message': 'Invalid jQuery selector or jQuery Object for "options.resultContainer".'
                        });

                        _debug.print();
                    }
                    // {/debug}
                } else {
                    this.resultContainer = this.options.resultContainer;
                }
            }

            if (this.options.maxItemPerGroup && this.options.group && this.options.group.key) {
                this.groupBy = this.options.group.key;
            }

            // Compatibility onClick callback
            if (this.options.callback && this.options.callback.onClick) {
                this.options.callback.onClickBefore = this.options.callback.onClick;
                delete this.options.callback.onClick;
            }

            // Compatibility onNavigate callback
            if (this.options.callback && this.options.callback.onNavigate) {
                this.options.callback.onNavigateBefore = this.options.callback.onNavigate;
                delete this.options.callback.onNavigate;
            }

            this.options = $.extend(
                true,
                {},
                _options,
                this.options
            );

        },

        unifySourceFormat: function () {

            this.groupCount = 0;

            // source: ['item1', 'item2', 'item3']
            if (Array.isArray(this.options.source)) {
                this.options.source = {
                    group: {
                        data: this.options.source
                    }
                };

                this.groupCount = 1;
                return true;
            }

            // source: "http://www.test.com/url.json"
            if (typeof this.options.source === "string") {
                this.options.source = {
                    group: {
                        ajax: {
                            url: this.options.source
                        }
                    }
                };
            }

            if (this.options.source.ajax) {
                this.options.source = {
                    group: {
                        ajax: this.options.source.ajax
                    }
                };
            }


            // source: {data: ['item1', 'item2'], url: "http://www.test.com/url.json"}
            if (this.options.source.url || this.options.source.data) {
                this.options.source = {
                    group: this.options.source
                };
            }

            var group,
                groupSource,
                tmpAjax;

            for (group in this.options.source) {
                if (!this.options.source.hasOwnProperty(group)) continue;

                groupSource = this.options.source[group];

                // source: {group: "http://www.test.com/url.json"}
                if (typeof groupSource === "string") {
                    groupSource = {
                        ajax: {
                            url: groupSource
                        }
                    };
                }

                // source: {group: {url: ["http://www.test.com/url.json", "json.path"]}}
                tmpAjax = groupSource.url || groupSource.ajax;
                if (Array.isArray(tmpAjax)) {
                    groupSource.ajax = typeof tmpAjax[0] === "string" ? {
                        url: tmpAjax[0]
                    } : tmpAjax[0];
                    groupSource.ajax.path = groupSource.ajax.path || tmpAjax[1] || null;
                    delete groupSource.url;
                } else {
                    // source: {group: {url: {url: "http://www.test.com/url.json", method: "GET"}}}
                    // source: {group: {url: "http://www.test.com/url.json", dataType: "jsonp"}}
                    if (typeof groupSource.url === "object") {
                        groupSource.ajax = groupSource.url;
                    } else if (typeof groupSource.url === "string") {
                        groupSource.ajax = {
                            url: groupSource.url
                        };
                    }
                    delete groupSource.url;
                }

                if (!groupSource.data && !groupSource.ajax) {

                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'unifySourceFormat()',
                            'arguments': JSON.stringify(this.options.source),
                            'message': 'Undefined "options.source.' + group + '.[data|ajax]" is Missing - Typeahead dropped'
                        });

                        _debug.print();
                    }
                    // {/debug}

                    return false;
                }

                if (groupSource.display && !Array.isArray(groupSource.display)) {
                    groupSource.display = [groupSource.display];
                }

                this.options.source[group] = groupSource;

                this.groupCount++;

            }

            return true;
        },

        init: function () {

            this.helper.executeCallback.call(this, this.options.callback.onInit, [this.node]);

            this.container = this.node.closest('.' + this.options.selector.container);

            // {debug}
            if (this.options.debug) {
                _debug.log({
                    'node': this.node.selector,
                    'function': 'init()',
                    //'arguments': JSON.stringify(this.options),
                    'message': 'OK - Typeahead activated on ' + this.node.selector
                });

                _debug.print();
            }
            // {/debug}

        },

        delegateEvents: function () {

            var scope = this,
                events = [
                    'focus' + this.namespace,
                    'input' + this.namespace,
                    'propertychange' + this.namespace,  // IE8 Fix
                    'keydown' + this.namespace,
                    'keyup' + this.namespace,           // IE9 Fix
                    'dynamic' + this.namespace,
                    'generate' + this.namespace
                ];

            // #149 - Adding support for Mobiles
            $('html').on("touchmove", function () {
                scope.hasDragged = true;
            }).on("touchstart", function () {
                scope.hasDragged = false;
            });

            this.node.closest('form').on("submit", function (e) {
                if (scope.options.mustSelectItem && scope.helper.isEmpty(scope.item)) {
                    e.preventDefault();
                    return;
                }

                if (!scope.options.backdropOnFocus) {
                    scope.hideLayout();
                }

                if (scope.options.callback.onSubmit) {
                    return scope.helper.executeCallback.call(scope, scope.options.callback.onSubmit, [scope.node, this, scope.item, e]);
                }
            }).on("reset", function () {
                // #221 - Reset Typeahead on form reset.
                // setTimeout to re-queue the `input.typeahead` event at the end
                setTimeout(function () {
                    scope.node.trigger('input' + scope.namespace);
                    // #243 - minLength: 0 opens the Typeahead results
                    scope.hideLayout();
                });
            });

            // IE8 fix
            var preventNextEvent = false;

            // IE10/11 fix
            if (this.node.attr('placeholder') && (_isIE10 || _isIE11)) {
                var preventInputEvent = true;

                this.node.on("focusin focusout", function () {
                    preventInputEvent = !!(!this.value && this.placeholder);
                });

                this.node.on("input", function (e) {
                    if (preventInputEvent) {
                        e.stopImmediatePropagation();
                        preventInputEvent = false;
                    }
                });
            }

            this.node.off(this.namespace).on(events.join(' '), function (e, originalEvent) {

                switch (e.type) {
                    case "generate":
                        scope.isGenerated = null;
                        scope.generateSource();
                        break;
                    case "focus":
                        if (scope.focusOnly) {
                            scope.focusOnly = false;
                            break;
                        }
                        if (scope.options.backdropOnFocus) {
                            scope.buildBackdropLayout();
                            scope.showLayout();
                        }
                        if (scope.options.searchOnFocus && scope.query.length >= scope.options.minLength) {
                            if (scope.isGenerated) {
                                scope.showLayout();
                            } else if (scope.isGenerated === null) {
                                scope.generateSource();
                            }
                        }
                        break;
                    case "keydown":
                        if (e.keyCode && ~[9, 13, 27, 38, 39, 40].indexOf(e.keyCode)) {
                            preventNextEvent = true;
                            scope.navigate(e);
                        }
                        break;
                    case "keyup":
                        if (scope.isGenerated === null && !scope.options.dynamic) {
                            scope.generateSource();
                        }
                        if (_isIE9 && scope.node[0].value.replace(/^\s+/, '').toString().length < scope.query.length) {
                            scope.node.trigger('input' + scope.namespace);
                        }
                        break;
                    case "propertychange":
                        if (preventNextEvent) {
                            preventNextEvent = false;
                            break;
                        }
                    case "input":

                        scope.rawQuery = scope.node[0].value.toString();

                        // #195 Trigger an onCancel event if the Typeahead is cleared
                        if (scope.rawQuery === "" && scope.query !== "") {
                            e.originalEvent = originalEvent || {};
                            scope.helper.executeCallback.call(scope, scope.options.callback.onCancel, [scope.node, e]);
                        }

                        scope.query = scope.rawQuery.replace(/^\s+/, '');

                        scope.options.cancelButton && scope.toggleCancelButton();

                        if (scope.options.hint && scope.hint.container && scope.hint.container.val() !== '') {
                            if (scope.hint.container.val().indexOf(scope.rawQuery) !== 0) {
                                scope.hint.container.val('');
                            }
                        }
                        if (scope.options.dynamic) {
                            scope.isGenerated = null;
                            scope.helper.typeWatch(function () {
                                if (scope.query.length >= scope.options.minLength) {
                                    scope.generateSource();
                                } else {
                                    scope.hideLayout();
                                }
                            }, scope.options.delay);
                            return;
                        }
                    case "dynamic":
                        if (!scope.isGenerated) {
                            break;
                        }

                        scope.searchResult();
                        scope.buildLayout();

                        if ((scope.result.length > 0 || (scope.options.emptyTemplate && scope.query !== "")) &&
                            scope.query.length >= scope.options.minLength
                        ) {
                            scope.showLayout();
                        } else {
                            scope.hideLayout();
                        }

                        break;
                }

            });

            if (this.options.generateOnLoad) {
                this.node.trigger('generate' + this.namespace);
            }

        },

        generateSource: function () {

            if (this.isGenerated && !this.options.dynamic) {
                return;
            }

            this.generatedGroupCount = 0;
            this.isGenerated = false;
            this.options.loadingAnimation && this.container.addClass('loading');

            if (!this.helper.isEmpty(this.xhr)) {
                for (var i in this.xhr) {
                    if (!this.xhr.hasOwnProperty(i)) continue;
                    this.xhr[i].abort();
                }
                this.xhr = {};
            }

            var scope = this,
                group,
                groupData,
                groupSource,
                dataInStorage,
                isValidStorage;

            for (group in this.options.source) {
                if (!this.options.source.hasOwnProperty(group)) continue;

                groupSource = this.options.source[group];

                // Get group source from Localstorage
                if (this.options.cache) {

                    dataInStorage = window[this.options.cache].getItem('TYPEAHEAD_' + this.node.selector + ":" + group);

                    if (dataInStorage) {
                        if (this.options.compression) {
                            dataInStorage = LZString.decompressFromUTF16(dataInStorage);
                        }

                        // In case the storage key:value are not readable anymore
                        isValidStorage = false;
                        try {
                            dataInStorage = JSON.parse(dataInStorage + "");

                            if (dataInStorage.data && dataInStorage.ttl > new Date().getTime()) {

                                this.populateSource(dataInStorage.data, group);
                                isValidStorage = true;

                                // {debug}
                                if (this.options.debug) {
                                    _debug.log({
                                        'node': this.node.selector,
                                        'function': 'generateSource()',
                                        'message': 'Source for group "' + group + '" found in ' + this.options.cache
                                    });
                                    _debug.print();
                                }
                                // {/debug}

                            } else {
                                window[this.options.cache].removeItem('TYPEAHEAD_' + this.node.selector + ":" + group);
                            }
                        } catch (error) {
                        }

                        if (isValidStorage) continue;
                    }
                }

                // Get group source from data
                if (groupSource.data && !groupSource.ajax) {

                    // #198 Add support for async data source
                    if (typeof groupSource.data === "function") {

                        groupData = groupSource.data.call(this);
                        if (Array.isArray(groupData)) {
                            scope.populateSource(groupData, group);
                        } else if (typeof groupData.promise === "function") {
                            (function (group) {
                                $.when(groupData).then(function (deferredData) {
                                    if (deferredData && Array.isArray(deferredData)) {
                                        scope.populateSource(deferredData, group);
                                    }
                                });
                            })(group);
                        }
                    } else {
                        this.populateSource(
                            $.extend(true, [], groupSource.data),
                            group
                        );
                    }

                    continue;
                }

                // Get group source from Ajax / JsonP
                if (groupSource.ajax) {
                    if (!this.requests[group]) {
                        this.requests[group] = this.generateRequestObject(group);
                    }
                }
            }

            this.handleRequests();

        },

        generateRequestObject: function (group) {

            var scope = this,
                groupSource = this.options.source[group];

            var xhrObject = {
                request: {
                    url: groupSource.ajax.url || null,
                    dataType: 'json',
                    beforeSend: function (jqXHR, options) {
                        // Important to call .abort() in case of dynamic requests
                        scope.xhr[group] = jqXHR;

                        var beforeSend = scope.requests[group].callback.beforeSend || groupSource.ajax.beforeSend;
                        typeof beforeSend === "function" && beforeSend.apply(null, arguments);
                    }
                },
                callback: {
                    beforeSend: null,
                    done: null,
                    fail: null,
                    then: null,
                    always: null
                },
                extra: {
                    path: groupSource.ajax.path || null,
                    group: group
                },
                validForGroup: [group]
            };

            if (typeof groupSource.ajax !== "function") {
                if (groupSource.ajax instanceof Object) {
                    xhrObject = this.extendXhrObject(xhrObject, groupSource.ajax);
                }

                if (Object.keys(this.options.source).length > 1) {
                    for (var _group in this.requests) {
                        if (!this.requests.hasOwnProperty(_group)) continue;
                        if (this.requests[_group].isDuplicated) continue;

                        if (xhrObject.request.url && xhrObject.request.url === this.requests[_group].request.url) {
                            this.requests[_group].validForGroup.push(group);
                            xhrObject.isDuplicated = true;
                            delete xhrObject.validForGroup;
                        }
                    }
                }
            }

            return xhrObject;
        },

        extendXhrObject: function (xhrObject, groupRequest) {

            if (typeof groupRequest.callback === "object") {
                xhrObject.callback = groupRequest.callback;
                delete groupRequest.callback;
            }

            // #132 Fixed beforeSend when using a function as the request object
            if (typeof groupRequest.beforeSend === "function") {
                xhrObject.callback.beforeSend = groupRequest.beforeSend;
                delete groupRequest.beforeSend;
            }

            // Fixes #105 Allow user to define their beforeSend function.
            // Fixes #181 IE8 incompatibility
            xhrObject.request = $.extend(
                true,
                xhrObject.request,
                groupRequest
            );

            // JSONP needs a unique jsonpCallback to run concurrently
            if (xhrObject.request.dataType.toLowerCase() === 'jsonp' && !xhrObject.request.jsonpCallback) {
                xhrObject.request.jsonpCallback = 'callback_' + xhrObject.extra.group;
            }

            return xhrObject;
        },

        handleRequests: function () {

            var scope = this,
                requestsCount = Object.keys(this.requests).length;

            if (this.helper.executeCallback.call(this, this.options.callback.onSendRequest, [this.node, this.query]) === false) {
                this.isGenerated = null;
                return;
            }

            for (var group in this.requests) {
                if (!this.requests.hasOwnProperty(group)) continue;
                if (this.requests[group].isDuplicated) continue;

                (function (group, xhrObject) {

                    if (typeof scope.options.source[group].ajax === "function") {

                        var _groupRequest = scope.options.source[group].ajax.call(scope, scope.query);

                        // Fixes #271 Data is cached inside the xhrObject
                        xhrObject = scope.extendXhrObject(
                            scope.generateRequestObject(group),
                            (typeof _groupRequest === "object") ? _groupRequest : {}
                        );

                        if (typeof xhrObject.request !== "object" || !xhrObject.request.url) {
                            // {debug}
                            if (scope.options.debug) {
                                _debug.log({
                                    'node': scope.node.selector,
                                    'function': 'handleRequests',
                                    'message': 'Source function must return an object containing ".url" key for group "' + group + '"'
                                });
                                _debug.print();
                            }
                            // {/debug}
                            scope.populateSource([], group);
                            return;
                        }

                        scope.requests[group] = xhrObject;
                    }

                    var _request,
                        _isExtended = false, // Prevent the main request from being changed
                        _data; // New data array in case it is modified inside callback.done

                    if (~xhrObject.request.url.indexOf('{{query}}')) {
                        if (!_isExtended) {
                            xhrObject = $.extend(true, {}, xhrObject);
                            _isExtended = true;
                        }
                        // #184 Invalid encoded characters on dynamic requests for `{{query}}`
                        xhrObject.request.url = xhrObject.request.url.replace('{{query}}', encodeURIComponent(scope.query));
                    }

                    if (xhrObject.request.data) {
                        for (var i in xhrObject.request.data) {
                            if (!xhrObject.request.data.hasOwnProperty(i)) continue;
                            if (~String(xhrObject.request.data[i]).indexOf('{{query}}')) {
                                if (!_isExtended) {
                                    xhrObject = $.extend(true, {}, xhrObject);
                                    _isExtended = true;
                                }
                                // jQuery handles encodeURIComponent when the query is inside the data object
                                xhrObject.request.data[i] = xhrObject.request.data[i].replace('{{query}}', scope.query);
                                break;
                            }
                        }
                    }

                    $.ajax(xhrObject.request).done(function (data, textStatus, jqXHR) {
                        _data = null;
                        for (var i = 0, ii = xhrObject.validForGroup.length; i < ii; i++) {

                            _request = scope.requests[xhrObject.validForGroup[i]];

                            if (_request.callback.done instanceof Function) {

                                _data = _request.callback.done(data, textStatus, jqXHR);

                                // {debug}
                                if (!Array.isArray(_data) || typeof _data !== "object") {
                                    if (scope.options.debug) {
                                        _debug.log({
                                            'node': scope.node.selector,
                                            'function': 'Ajax.callback.done()',
                                            'message': 'Invalid returned data has to be an Array'
                                        });
                                        _debug.print();
                                    }
                                }
                                // {/debug}
                            }
                        }

                    }).fail(function (jqXHR, textStatus, errorThrown) {

                        for (var i = 0, ii = xhrObject.validForGroup.length; i < ii; i++) {
                            _request = scope.requests[xhrObject.validForGroup[i]];
                            _request.callback.fail instanceof Function && _request.callback.fail(jqXHR, textStatus, errorThrown);
                        }

                        // {debug}
                        if (scope.options.debug) {
                            _debug.log({
                                'node': scope.node.selector,
                                'function': 'Ajax.callback.fail()',
                                'arguments': JSON.stringify(xhrObject.request),
                                'message': textStatus
                            });

                            console.log(errorThrown);

                            _debug.print();
                        }
                        // {/debug}

                    }).always(function (data, textStatus, jqXHR) {

                        for (var i = 0, ii = xhrObject.validForGroup.length; i < ii; i++) {
                            _request = scope.requests[xhrObject.validForGroup[i]];
                            _request.callback.always instanceof Function && _request.callback.always(data, textStatus, jqXHR);

                            // #248 Aborted requests would call populate with invalid data
                            // #265 Modified data from ajax.callback.done is not being registred (use of _data)
                            scope.populateSource(
                                typeof data.promise === "function" && [] || _data || data,
                                _request.extra.group,
                                _request.extra.path || _request.request.path
                            );

                            requestsCount -= 1;
                            if (requestsCount === 0) {
                                scope.helper.executeCallback.call(scope, scope.options.callback.onReceiveRequest, [scope.node, scope.query]);
                            }

                        }

                    }).then(function (jqXHR, textStatus) {

                        for (var i = 0, ii = xhrObject.validForGroup.length; i < ii; i++) {
                            _request = scope.requests[xhrObject.validForGroup[i]];
                            _request.callback.then instanceof Function && _request.callback.then(jqXHR, textStatus);
                        }

                    });

                }(group, this.requests[group]));

            }

        },

        /**
         * Build the source groups to be cycled for matched results
         *
         * @param {Array} data Array of Strings or Array of Objects
         * @param {String} group
         * @param {String} [path]
         * @return {*}
         */
        populateSource: function (data, group, path) {

            var scope = this,
                groupSource = this.options.source[group],
                extraData = groupSource.ajax && groupSource.data;

            data = typeof path === "string" ? this.helper.namespace(path, data) : data;

            if (typeof data === 'undefined') {
                // {debug}
                if (this.options.debug) {
                    _debug.log({
                        'node': this.node.selector,
                        'function': 'populateSource()',
                        'arguments': path,
                        'message': 'Invalid data path.'
                    });

                    _debug.print();
                }
                // {/debug}
            }

            if (!Array.isArray(data)) {
                // {debug}
                if (this.options.debug) {
                    _debug.log({
                        'node': this.node.selector,
                        'function': 'populateSource()',
                        'arguments': JSON.stringify({group: group}),
                        'message': 'Invalid data type, must be Array type.'
                    });
                    _debug.print();
                }
                // {/debug}
                data = [];
            }

            if (extraData) {
                if (typeof extraData === "function") {
                    extraData = extraData();
                }

                if (Array.isArray(extraData)) {
                    data = data.concat(extraData);
                }
                // {debug}
                else {
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'populateSource()',
                            'arguments': JSON.stringify(extraData),
                            'message': 'WARNING - this.options.source.' + group + '.data Must be an Array or a function that returns an Array.'
                        });

                        _debug.print();
                    }
                }
                // {/debug}
            }

            var tmpObj,
                display = groupSource.display ?
                    (groupSource.display[0] === 'compiled' ? groupSource.display[1] : groupSource.display[0]) :
                    (this.options.display[0] === 'compiled' ? this.options.display[1] : this.options.display[0]);

            for (var i = 0, ii = data.length; i < ii; i++) {
                if (data[i] === null || typeof data[i] === "boolean") {
                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'populateSource()',
                            'message': 'WARNING - NULL/BOOLEAN value inside ' + group + '! The data was skipped.'
                        });

                        _debug.print();
                    }
                    // {/debug}
                    continue;
                }
                if (typeof data[i] === "string") {
                    tmpObj = {};
                    tmpObj[display] = data[i];
                    data[i] = tmpObj;
                }
                data[i].group = group;
            }

            if (!this.options.dynamic && this.dropdownFilter.dynamic.length) {

                var key,
                    value,
                    tmpValues = {};

                for (var i = 0, ii = data.length; i < ii; i++) {
                    for (var k = 0, kk = this.dropdownFilter.dynamic.length; k < kk; k++) {
                        key = this.dropdownFilter.dynamic[k].key;

                        value = data[i][key];
                        if (!value) continue;
                        if (!this.dropdownFilter.dynamic[k].value) {
                            this.dropdownFilter.dynamic[k].value = [];
                        }
                        if (!tmpValues[key]) {
                            tmpValues[key] = [];
                        }
                        if (!~tmpValues[key].indexOf(value.toLowerCase())) {
                            tmpValues[key].push(value.toLowerCase());
                            this.dropdownFilter.dynamic[k].value.push(value);
                        }
                    }
                }
            }

            if (this.options.correlativeTemplate) {

                var template = groupSource.template || this.options.template,
                    compiledTemplate = "";

                if (typeof template === "function") {
                    template = template.call(this, '', {});
                }

                if (!template) {
                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'populateSource()',
                            'arguments': JSON.stringify(group),
                            'message': 'WARNING - this.options.correlativeTemplate is enabled but no template was found.'
                        });

                        _debug.print();
                    }
                    // {/debug}
                } else {

                    // #109 correlativeTemplate can be an array of display keys instead of the complete template
                    if (Array.isArray(this.options.correlativeTemplate)) {
                        for (var i = 0, ii = this.options.correlativeTemplate.length; i < ii; i++) {
                            compiledTemplate += "{{" + this.options.correlativeTemplate[i] + "}} ";
                        }
                    } else {
                        compiledTemplate = template
                            .replace(/<.+?>/g, '');
                    }

                    for (var i = 0, ii = data.length; i < ii; i++) {
                        data[i].compiled = compiledTemplate.replace(/\{\{([\w\-\.]+)(?:\|(\w+))?}}/g, function (match, index) {
                                return scope.helper.namespace(index, data[i], 'get', '');
                            }
                        ).trim();
                    }

                    if (groupSource.display) {
                        if (!~groupSource.display.indexOf('compiled')) {
                            groupSource.display.unshift('compiled');
                        }
                    } else if (!~this.options.display.indexOf('compiled')) {
                        this.options.display.unshift('compiled');
                    }

                }
            }

            if (this.options.callback.onPopulateSource) {
                data = this.helper.executeCallback.call(this, this.options.callback.onPopulateSource, [this.node, data, group, path]);

                // {debug}
                if (this.options.debug) {
                    if (!data || !Array.isArray(data)) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'callback.populateSource()',
                            'message': 'callback.onPopulateSource must return the "data" parameter'
                        });

                        _debug.print();
                    }
                }
                // {/debug}
            }

            // Save the data inside a tmpSource var to later have the right order once every request are completed
            this.tmpSource[group] = data;

            if (this.options.cache && !window[this.options.cache].getItem('TYPEAHEAD_' + this.node.selector + ":" + group)) {

                if (this.options.callback.onCacheSave) {
                    data = this.helper.executeCallback.call(this, this.options.callback.onCacheSave, [this.node, data, group, path]);

                    // {debug}
                    if (this.options.debug) {
                        if (!data || !Array.isArray(data)) {
                            _debug.log({
                                'node': this.node.selector,
                                'function': 'callback.populateSource()',
                                'message': 'callback.onCacheSave must return the "data" parameter'
                            });

                            _debug.print();
                        }
                    }
                    // {/debug}
                }

                var storage = JSON.stringify({
                    data: data,
                    ttl: new Date().getTime() + this.options.ttl
                });

                if (this.options.compression) {
                    storage = LZString.compressToUTF16(storage);
                }

                window[this.options.cache].setItem(
                    'TYPEAHEAD_' + this.node.selector + ":" + group,
                    storage
                );
            }

            this.incrementGeneratedGroup();

        },

        incrementGeneratedGroup: function () {

            this.generatedGroupCount++;

            if (this.groupCount !== this.generatedGroupCount) {
                return;
            }

            this.isGenerated = true;

            this.xhr = {};

            var sourceKeys = Object.keys(this.options.source);

            for (var i = 0, ii = sourceKeys.length; i < ii; i++) {
                this.source[sourceKeys[i]] = this.tmpSource[sourceKeys[i]];
            }

            this.tmpSource = {};

            if (!this.options.dynamic) {
                this.buildDropdownItemLayout('dynamic');
            }

            this.options.loadingAnimation && this.container.removeClass('loading');

            this.node.trigger('dynamic' + this.namespace);

        },

        /**
         * Key Navigation
         * tab 9: @TODO, what should tab do?
         * Up 38: select previous item, skip "group" item
         * Down 40: select next item, skip "group" item
         * Right 39: change charAt, if last char fill hint (if options is true)
         * Esc 27: clears input (is not empty) / blur (if empty)
         * Enter 13: Select item + submit search
         *
         * @param {Object} e Event object
         * @returns {*}
         */
        navigate: function (e) {

            this.helper.executeCallback.call(this, this.options.callback.onNavigateBefore, [this.node, this.query, e]);

            if (e.keyCode === 27) {
                // #166 Different browsers do not have the same behaviors by default, lets enforce what we want instead
                e.preventDefault();
                if (this.query.length) {
                    this.resetInput();
                    this.node.trigger('input' + this.namespace, [e]);
                } else {
                    this.node.blur();
                    this.hideLayout();
                }
                return;
            }

            if (!this.isGenerated || !this.result.length) return;

            var itemList = this.resultContainer.find('.' + this.options.selector.item),
                activeItem = itemList.filter('.active'),
                activeItemIndex = activeItem[0] && itemList.index(activeItem) || null,
                newActiveItemIndex = null;

            if (e.keyCode === 13) {
                if (activeItem.length > 0) {
                    // Prevent form submit if an element is selected
                    e.preventDefault();
                    activeItem.find('a:first').trigger('click', e);
                }
                return;
            }

            if (e.keyCode === 39) {
                if (activeItemIndex) {
                    itemList.eq(activeItemIndex).find('a:first')[0].click();
                } else if (this.options.hint &&
                    this.hint.container.val() !== "" &&
                    this.helper.getCaret(this.node[0]) >= this.query.length) {

                    itemList.find('a[data-index="' + this.hintIndex + '"]')[0].click();

                }
                return;
            }

            if (itemList.length > 0) {
                activeItem.removeClass('active');
            }

            if (e.keyCode === 38) {

                e.preventDefault();

                if (activeItem.length > 0) {
                    if (activeItemIndex - 1 >= 0) {
                        newActiveItemIndex = activeItemIndex - 1;
                        itemList.eq(newActiveItemIndex).addClass('active');
                    }
                } else {
                    newActiveItemIndex = itemList.length - 1;
                    itemList.last().addClass('active');
                }

            } else if (e.keyCode === 40) {

                e.preventDefault();

                if (activeItem.length > 0) {
                    if (activeItemIndex + 1 < itemList.length) {
                        newActiveItemIndex = activeItemIndex + 1;
                        itemList.eq(newActiveItemIndex).addClass('active');
                    }
                } else {
                    newActiveItemIndex = 0;
                    itemList.first().addClass('active');
                }
            }

            // #115 Prevent the input from changing when navigating (arrow up / down) the results
            if (e.preventInputChange && ~[38, 40].indexOf(e.keyCode)) {
                this.buildHintLayout(
                    newActiveItemIndex !== null && newActiveItemIndex < this.result.length ?
                        [this.result[newActiveItemIndex]] :
                        null
                );
            }

            if (this.options.hint && this.hint.container) {
                this.hint.container.css(
                    'color',
                    e.preventInputChange ?
                        this.hint.css.color :
                    newActiveItemIndex === null && this.hint.css.color || this.hint.container.css('background-color') || 'fff'
                );
            }

            this.node.val(
                newActiveItemIndex !== null && !e.preventInputChange ?
                    this.result[newActiveItemIndex][this.result[newActiveItemIndex].matchedKey] :
                    this.rawQuery
            );

            this.helper.executeCallback.call(this, this.options.callback.onNavigateAfter, [
                this.node,
                itemList,
                newActiveItemIndex !== null && itemList.eq(newActiveItemIndex).find('a:first') || undefined,
                newActiveItemIndex !== null && this.result[newActiveItemIndex] || undefined,
                this.query,
                e
            ]);

        },

        searchResult: function (preserveItem) {

            // #54 In case the item is being clicked, we want to preserve it for onSubmit callback
            if (!preserveItem) {
                this.item = {};
            }

            this.resetLayout();

            if (this.helper.executeCallback.call(this, this.options.callback.onSearch, [this.node, this.query]) === false) return;

            if (this.query.length >= this.options.minLength) {
                this.searchResultData();
            }

            this.helper.executeCallback.call(this, this.options.callback.onResult, [this.node, this.query, this.result, this.resultCount, this.resultCountPerGroup]);

            if (this.isDropdownEvent) {
                this.helper.executeCallback.call(this, this.options.callback.onDropdownFilter, [this.node, this.query, this.filters.dropdown, this.result]);
                this.isDropdownEvent = false;
            }
        },

        searchResultData: function () {

            var scope = this,
                group,
                groupBy = this.groupBy,
                groupReference = null,
                item,
                match,
                comparedDisplay,
                comparedQuery = this.query.toLowerCase(),
                maxItemPerGroup = this.options.maxItemPerGroup,
                hasDynamicFilters = this.filters.dynamic && !this.helper.isEmpty(this.filters.dynamic),
                displayKeys,
                displayValue,
                missingDisplayKey = {},
                groupFilter,
                groupFilterResult,
                groupMatcher,
                groupMatcherResult,
                matcher = typeof this.options.matcher === "function" && this.options.matcher,
                correlativeMatch,
                correlativeQuery,
                correlativeDisplay;

            if (this.options.accent) {
                comparedQuery = this.helper.removeAccent.call(this, comparedQuery);
            }

            for (group in this.source) {

                if (!this.source.hasOwnProperty(group)) continue;
                // dropdownFilter by source groups
                if (this.filters.dropdown && this.filters.dropdown.key === "group" && this.filters.dropdown.value !== group) continue;

                groupFilter = typeof this.options.source[group].filter !== "undefined" ? this.options.source[group].filter : this.options.filter;
                groupMatcher = typeof this.options.source[group].matcher === "function" && this.options.source[group].matcher || matcher;

                for (var k = 0, kk = this.source[group].length; k < kk; k++) {
                    if (this.result.length >= this.options.maxItem && !this.options.callback.onResult) break;
                    if (hasDynamicFilters && !this.dynamicFilter.validate.apply(this, [this.source[group][k]])) continue;

                    item = this.source[group][k];
                    // Validation over null item
                    if (item === null || typeof item === "boolean") continue;

                    // dropdownFilter by custom groups
                    if (this.filters.dropdown && (item[this.filters.dropdown.key] || "").toLowerCase() !== (this.filters.dropdown.value || "").toLowerCase()) continue;

                    groupReference = groupBy === "group" ? group : item[groupBy];

                    if (groupReference && !this.result[groupReference]) {
                        this.result[groupReference] = [];
                        this.resultCountPerGroup[groupReference] = 0;
                    }

                    if (maxItemPerGroup) {
                        if (groupBy === "group" && this.result[groupReference].length >= maxItemPerGroup && !this.options.callback.onResult) {
                            break;
                        }
                    }

                    displayKeys = this.options.source[group].display || this.options.display;

                    for (var i = 0, ii = displayKeys.length; i < ii; i++) {

                        // #183 Allow searching for deep source object keys
                        displayValue = /\./.test(displayKeys[i]) ?
                            this.helper.namespace(displayKeys[i], item) :
                            item[displayKeys[i]];

                        // #182 Continue looping if empty or undefined key
                        if (typeof displayValue === 'undefined' || displayValue === '') {
                            // {debug}
                            if (this.options.debug) {
                                missingDisplayKey[i] = {
                                    display: displayKeys[i],
                                    data: item
                                };
                            }
                            // {/debug}
                            continue;
                        }

                        displayValue = this.helper.cleanStringFromScript(displayValue);

                        if (typeof groupFilter === "function") {
                            groupFilterResult = groupFilter.call(this, item, displayValue);

                            // return undefined to skip to next item
                            // return false to attempt the matching function on the next displayKey
                            // return true to add the item to the result list
                            // return item object to modify the item and add it to the result list

                            if (groupFilterResult === undefined) break;
                            if (!groupFilterResult) continue;
                            if (typeof groupFilterResult === "object") {
                                item = groupFilterResult;
                            }
                        }

                        if (~[undefined, true].indexOf(groupFilter)) {
                            comparedDisplay = displayValue;
                            comparedDisplay = comparedDisplay.toString().toLowerCase();

                            if (this.options.accent) {
                                comparedDisplay = this.helper.removeAccent.call(this, comparedDisplay);
                            }

                            match = comparedDisplay.indexOf(comparedQuery);

                            if (this.options.correlativeTemplate && displayKeys[i] === 'compiled' && match < 0 && /\s/.test(comparedQuery)) {
                                correlativeMatch = true;
                                correlativeQuery = comparedQuery.split(' ');
                                correlativeDisplay = comparedDisplay;
                                for (var x = 0, xx = correlativeQuery.length; x < xx; x++) {
                                    if (correlativeQuery[x] === "") continue;
                                    if (!~correlativeDisplay.indexOf(correlativeQuery[x])) {
                                        correlativeMatch = false;
                                        break;
                                    }
                                    correlativeDisplay = correlativeDisplay.replace(correlativeQuery[x], '');
                                }
                            }

                            if (match < 0 && !correlativeMatch) continue;
                            // @TODO Deprecate these? use matcher instead?
                            if (this.options.offset && match !== 0) continue;

                            if (groupMatcher) {
                                groupMatcherResult = groupMatcher.call(this, item, displayValue);

                                // return undefined to skip to next item
                                // return false to attempt the matching function on the next displayKey
                                // return true to add the item to the result list
                                // return item object to modify the item and add it to the result list

                                if (groupMatcherResult === undefined) break;
                                if (!groupMatcherResult) continue;
                                if (typeof groupMatcherResult === "object") {
                                    item = groupMatcherResult;
                                }
                            }
                        }

                        this.resultCount++;
                        this.resultCountPerGroup[groupReference]++;

                        if (this.resultItemCount < this.options.maxItem) {
                            if (maxItemPerGroup && this.result[groupReference].length >= maxItemPerGroup) {
                                break;
                            }

                            item.matchedKey = displayKeys[i];

                            this.result[groupReference].push(item);
                            this.resultItemCount++;
                        }
                        break;
                    }

                    if (!this.options.callback.onResult) {
                        if (this.resultItemCount >= this.options.maxItem) {
                            break;
                        }
                        if (maxItemPerGroup && this.result[groupReference].length >= maxItemPerGroup) {
                            if (groupBy === "group") {
                                break;
                            }
                        }
                    }
                }
            }

            // {debug}
            if (this.options.debug) {
                if (!this.helper.isEmpty(missingDisplayKey)) {
                    _debug.log({
                        'node': this.node.selector,
                        'function': 'searchResult()',
                        'arguments': JSON.stringify(missingDisplayKey),
                        'message': 'Missing keys for display, make sure options.display is set properly.'
                    });

                    _debug.print();
                }
            }
            // {/debug}

            if (this.options.order) {

                var displayKeys = [],
                    displayKey;

                for (var group in this.result) {
                    if (!this.result.hasOwnProperty(group)) continue;
                    for (var i = 0, ii = this.result[group].length; i < ii; i++) {
                        displayKey = this.options.source[this.result[group][i].group].display || this.options.display;
                        if (!~displayKeys.indexOf(displayKey[0])) {
                            displayKeys.push(displayKey[0]);
                        }
                    }
                    this.result[group].sort(
                        scope.helper.sort(
                            displayKeys,
                            scope.options.order === "asc",
                            function (a) {
                                return a.toString().toUpperCase();
                            }
                        )
                    );
                }

            }

            var concatResults = [],
                groupOrder;

            if (typeof this.options.groupOrder === "function") {
                groupOrder = this.options.groupOrder.apply(this, [this.node, this.query, this.result, this.resultCount, this.resultCountPerGroup]);
            } else if (Array.isArray(this.options.groupOrder)) {
                groupOrder = this.options.groupOrder;
            } else if (typeof this.options.groupOrder === "string" && ~["asc", "desc"].indexOf(this.options.groupOrder)) {
                groupOrder = Object.keys(this.result).sort(
                    scope.helper.sort(
                        [],
                        scope.options.groupOrder === "asc",
                        function (a) {
                            return a.toString().toUpperCase();
                        }
                    )
                );
            } else {
                groupOrder = Object.keys(this.result);
            }

            this.groups = groupOrder;

            for (var i = 0, ii = groupOrder.length; i < ii; i++) {
                concatResults = concatResults.concat(this.result[groupOrder[i]] || []);
            }

            this.result = concatResults;
        },

        buildLayout: function () {

            this.buildHtmlLayout();

            this.buildBackdropLayout();

            this.buildHintLayout();

            if (this.options.callback.onLayoutBuiltBefore) {
                var tmpResultHtml = this.helper.executeCallback.call(this, this.options.callback.onLayoutBuiltBefore, [this.node, this.query, this.result, this.resultHtml]);

                if (tmpResultHtml instanceof $) {
                    this.resultHtml = tmpResultHtml;
                }
                // {debug}
                else {
                    if (this.options.debug) {
                        _debug.log({
                            'node': this.node.selector,
                            'function': 'callback.onLayoutBuiltBefore()',
                            'message': 'Invalid returned value - You must return resultHtmlList jQuery Object'
                        });

                        _debug.print();
                    }
                }
                // {/debug}
            }

            this.resultHtml && this.resultContainer.html(this.resultHtml);

            if (this.options.callback.onLayoutBuiltAfter) {
                this.helper.executeCallback.call(this, this.options.callback.onLayoutBuiltAfter, [this.node, this.query, this.result]);
            }
        },

        buildHtmlLayout: function () {
            // #150 Add the option to have no resultList but still perform the search and trigger the callbacks
            if (this.options.resultContainer === false) return;

            if (!this.resultContainer) {
                this.resultContainer = $("<div/>", {
                    "class": this.options.selector.result
                });

                this.container.append(this.resultContainer);
            }

            var emptyTemplate;
            if (!this.result.length) {
                if (this.options.emptyTemplate && this.query !== "") {
                    emptyTemplate = typeof this.options.emptyTemplate === "function" ?
                        this.options.emptyTemplate.call(this, this.query) :
                        this.options.emptyTemplate.replace(/\{\{query}}/gi, this.helper.cleanStringFromScript(this.query));

                } else {
                    return;
                }
            }

            var _query = this.query.toLowerCase();
            if (this.options.accent) {
                _query = this.helper.removeAccent.call(this, _query);
            }

            var scope = this,
                groupTemplate = this.groupTemplate || '<ul></ul>',
                hasEmptyTemplate = false;

            if (this.groupTemplate) {
                groupTemplate = $(groupTemplate.replace(/<([^>]+)>\{\{(.+?)}}<\/[^>]+>/g, function (match, tag, group, offset, string) {
                    var template = '',
                        groups = group === "group" ? scope.groups : [group];

                    if (!scope.result.length) {
                        if (hasEmptyTemplate === true) return '';
                        hasEmptyTemplate = true;

                        return '<' + tag + ' class="' + scope.options.selector.empty + '"><a href="javascript:;">' + emptyTemplate + '</a></' + tag + '>';
                    }

                    for (var i = 0, ii = groups.length; i < ii; ++i) {
                        template += '<' + tag + ' data-group-template="' + groups[i] + '"><ul></ul></' + tag + '>';
                    }

                    return template;
                }));
            } else {
                groupTemplate = $(groupTemplate);
                if (!this.result.length) {
                    groupTemplate.append(
                        emptyTemplate instanceof $ ?
                            emptyTemplate :
                        '<li class="' + scope.options.selector.empty + '"><a href="javascript:;">' + emptyTemplate + '</a></li>'
                    );
                }
            }

            groupTemplate.addClass(this.options.selector.list + (this.helper.isEmpty(this.result) ? ' empty' : ''));

            var _group,
                _groupTemplate,
                _item,
                _href,
                _liHtml,
                _template,
                _aHtml,
                _display,
                _displayKeys,
                _displayValue,
                _unusedGroups = this.groupTemplate && this.result.length && scope.groups || [],
                _tmpIndexOf;

            for (var i = 0, ii = this.result.length; i < ii; ++i) {

                _item = this.result[i];
                _group = _item.group;
                _href = this.options.source[_item.group].href || this.options.href;
                _display = [];
                _displayKeys = this.options.source[_item.group].display || this.options.display;

                // @TODO Optimize this, shouldn't occur on every looped item?
                if (this.options.group) {
                    _group = _item[this.options.group.key];
                    if (this.options.group.template) {
                        if (typeof this.options.group.template === "function") {
                            _groupTemplate = this.options.group.template(_item);
                        } else if (typeof this.options.template === "string") {
                            _groupTemplate = this.options.group.template.replace(/\{\{([\w\-\.]+)}}/gi, function (match, index) {
                                return scope.helper.namespace(index, _item, 'get', '');
                            });
                        }
                    }

                    if (!groupTemplate.find('[data-search-group="' + _group + '"]')[0]) {
                        (this.groupTemplate ? groupTemplate.find('[data-group-template="' + _group + '"] ul') : groupTemplate).append(
                            $("<li/>", {
                                "class": scope.options.selector.group,
                                "html": $("<a/>", {
                                    "href": "javascript:;",
                                    "html": _groupTemplate || _group,
                                    "tabindex": -1
                                }),
                                "data-search-group": _group
                            })
                        );
                    }
                }

                if (this.groupTemplate && _unusedGroups.length) {
                    _tmpIndexOf = _unusedGroups.indexOf(_group || _item.group);
                    if (~_tmpIndexOf) {
                        _unusedGroups.splice(_tmpIndexOf, 1);
                    }
                }

                _liHtml = $("<li/>", {
                    "class": scope.options.selector.item + " " + scope.options.selector.group + '-' + this.helper.slugify.call(this, _group),
                    "html": $("<a/>", {
                        // #190 Strange JS-code fragment in href attribute using jQuery version below 1.10
                        "href": (function () {
                            if (_href) {
                                if (typeof _href === "string") {
                                    _href = _href.replace(/\{\{([^\|}]+)(?:\|([^}]+))*}}/gi, function (match, index, options) {

                                        var value = scope.helper.namespace(index, _item, 'get', '');

                                        // #151 Slugify should be an option, not enforced
                                        options = options && options.split("|") || [];
                                        if (~options.indexOf('slugify')) {
                                            value = scope.helper.slugify.call(scope, value);
                                        }

                                        return value;
                                    });
                                } else if (typeof _href === "function") {
                                    _href = _href(_item);
                                }
                                _item.href = _href;
                            }
                            return _href || "javascript:;";
                        }()),
                        "data-group": _group,
                        "data-index": i,
                        "html": function () {

                            _template = (_item.group && scope.options.source[_item.group].template) || scope.options.template;

                            if (_template) {
                                if (typeof _template === "function") {
                                    _template = _template.call(scope, scope.query, _item);
                                }

                                _aHtml = _template.replace(/\{\{([^\|}]+)(?:\|([^}]+))*}}/gi, function (match, index, options) {

                                    var value = scope.helper.cleanStringFromScript(String(scope.helper.namespace(index, _item, 'get', '')));

                                    // #151 Slugify should be an option, not enforced
                                    options = options && options.split("|") || [];
                                    if (~options.indexOf('slugify')) {
                                        value = scope.helper.slugify.call(scope, value);
                                    }

                                    if (!~options.indexOf('raw')) {
                                        if (scope.options.highlight === true && _query && ~_displayKeys.indexOf(index)) {
                                            value = scope.helper.highlight.call(scope, value, _query.split(" "), scope.options.accent);
                                        }
                                    }
                                    return value;
                                });
                            } else {
                                for (var i = 0, ii = _displayKeys.length; i < ii; i++) {
                                    _displayValue = /\./.test(_displayKeys[i]) ?
                                        scope.helper.namespace(_displayKeys[i], _item) :
                                        _item[_displayKeys[i]];

                                    if (typeof _displayValue === 'undefined' || _displayValue === '') continue;

                                    _display.push(_displayValue);
                                }

                                _aHtml = '<span class="' + scope.options.selector.display + '">' + scope.helper.cleanStringFromScript(String(_display.join(" "))) + '</span>';
                            }

                            if ((scope.options.highlight === true && _query && !_template) || scope.options.highlight === "any") {
                                _aHtml = scope.helper.highlight.call(scope, _aHtml, _query.split(" "), scope.options.accent);
                            }

                            $(this).append(_aHtml);

                        }
                    })
                });

                (function (i, item, liHtml) {
                    liHtml.on('click', function (e, originalEvent) {
                        // #208 - Attach "keyboard Enter" original event
                        if (originalEvent && typeof originalEvent === "object") {
                            e.originalEvent = originalEvent;
                        }

                        if (scope.options.mustSelectItem && scope.helper.isEmpty(item)) {
                            e.preventDefault();
                            return;
                        }

                        scope.item = item;

                        if (scope.helper.executeCallback.call(scope, scope.options.callback.onClickBefore, [scope.node, $(this), item, e]) === false) return;
                        if ((e.originalEvent && e.originalEvent.defaultPrevented) || e.isDefaultPrevented()) {
                            return;
                        }

                        scope.query = scope.rawQuery = item[item.matchedKey].toString();

                        scope.focusOnly = true;
                        scope.node.val(scope.query).focus();

                        scope.searchResult(true);
                        scope.buildLayout();
                        scope.hideLayout();

                        scope.helper.executeCallback.call(scope, scope.options.callback.onClickAfter, [scope.node, $(this), item, e]);
                    });
                    liHtml.on('mouseenter', function (e) {
                        scope.helper.executeCallback.call(scope, scope.options.callback.onMouseEnter, [scope.node, $(this), item, e]);
                    });
                    liHtml.on('mouseleave', function (e) {
                        scope.helper.executeCallback.call(scope, scope.options.callback.onMouseLeave, [scope.node, $(this), item, e]);
                    });
                }(i, _item, _liHtml));

                (this.groupTemplate ? groupTemplate.find('[data-group-template="' + _group + '"] ul') : groupTemplate).append(_liHtml);
            }

            if (this.result.length && _unusedGroups.length) {
                for (var i = 0, ii = _unusedGroups.length; i < ii; ++i) {
                    groupTemplate.find('[data-group-template="' + _unusedGroups[i] + '"]').remove();
                }
            }

            this.resultHtml = groupTemplate;

        },

        buildBackdropLayout: function () {

            if (!this.options.backdrop) return;

            if (!this.backdrop.container) {
                this.backdrop.css = $.extend(
                    {
                        "opacity": 0.6,
                        "filter": 'alpha(opacity=60)',
                        "position": 'fixed',
                        "top": 0,
                        "right": 0,
                        "bottom": 0,
                        "left": 0,
                        "z-index": 1040,
                        "background-color": "#000"
                    },
                    this.options.backdrop
                );

                this.backdrop.container = $("<div/>", {
                    "class": this.options.selector.backdrop,
                    "css": this.backdrop.css
                }).insertAfter(this.container);

            }
            this.container
                .addClass('backdrop')
                .css({
                    "z-index": this.backdrop.css["z-index"] + 1,
                    "position": "relative"
                });

        },

        buildHintLayout: function (result) {
            if (!this.options.hint) return;
            // #144 hint doesn't overlap with the input when the query is too long
            if (this.node[0].scrollWidth > Math.ceil(this.node.innerWidth())) {
                this.hint.container && this.hint.container.val("");
                return;
            }

            var scope = this,
                hint = "",
                result = result || this.result,
                query = this.query.toLowerCase();

            if (this.options.accent) {
                query = this.helper.removeAccent.call(this, query);
            }

            this.hintIndex = null;

            if (this.query.length >= this.options.minLength) {

                if (!this.hint.container) {

                    this.hint.css = $.extend({
                            "border-color": "transparent",
                            "position": "absolute",
                            "top": 0,
                            "display": "inline",
                            "z-index": -1,
                            "float": "none",
                            "color": "silver",
                            "box-shadow": "none",
                            "cursor": "default",
                            "-webkit-user-select": "none",
                            "-moz-user-select": "none",
                            "-ms-user-select": "none",
                            "user-select": "none"
                        },
                        this.options.hint
                    );

                    this.hint.container = $('<input/>', {
                        'type': this.node.attr('type'),
                        'class': this.node.attr('class'),
                        'readonly': true,
                        'unselectable': 'on',
                        'aria-hidden': 'true',
                        'tabindex': -1,
                        'click': function () {
                            // IE8 Fix
                            scope.node.focus();
                        }
                    }).addClass(this.options.selector.hint)
                        .css(this.hint.css)
                        .insertAfter(this.node);

                    this.node.parent().css({
                        "position": "relative"
                    });
                }

                this.hint.container.css('color', this.hint.css.color);

                // Do not display hint for empty query
                if (query) {
                    var _displayKeys,
                        _group,
                        _comparedValue;

                    for (var i = 0, ii = result.length; i < ii; i++) {

                        _group = result[i].group;
                        _displayKeys = this.options.source[_group].display || this.options.display;

                        for (var k = 0, kk = _displayKeys.length; k < kk; k++) {

                            _comparedValue = String(result[i][_displayKeys[k]]).toLowerCase();
                            if (this.options.accent) {
                                _comparedValue = this.helper.removeAccent.call(this, _comparedValue);
                            }

                            if (_comparedValue.indexOf(query) === 0) {
                                hint = String(result[i][_displayKeys[k]]);
                                this.hintIndex = i;
                                break;
                            }
                        }
                        if (this.hintIndex !== null) {
                            break;
                        }
                    }
                }

                this.hint.container
                    .val(hint.length > 0 && this.rawQuery + hint.substring(this.query.length) || "");

            }

        },

        buildDropdownLayout: function () {

            if (!this.options.dropdownFilter) {
                return;
            }

            var scope = this;

            $('<span/>', {
                "class": this.options.selector.filter,
                "html": function () {

                    $(this).append(
                        $('<button/>', {
                            "type": "button",
                            "class": scope.options.selector.filterButton,
                            "style": "display: none;",
                            "click": function (e) {
                                e.stopPropagation();
                                scope.container.toggleClass('filter');

                                var _ns = scope.namespace + '-dropdown-filter';

                                $('html').off(_ns);

                                if (scope.container.hasClass('filter')) {
                                    $('html').on("click" + _ns + " touchend" + _ns, function (e) {
                                        if ($(e.target).closest('.' + scope.options.selector.filter)[0] || scope.hasDragged) return;
                                        scope.container.removeClass('filter');
                                    });
                                }
                            }
                        })
                    );

                    $(this).append(
                        $('<ul/>', {
                            "class": scope.options.selector.dropdown
                        })
                    );
                }
            }).insertAfter(scope.container.find('.' + scope.options.selector.query));

        },

        buildDropdownItemLayout: function (type) {

            var scope = this,
                template,
                all = typeof this.options.dropdownFilter === 'string' && this.options.dropdownFilter || 'All',
                ulScope = this.container.find('.' + this.options.selector.dropdown),
                filter;

            // Use regular groups defined in options.source
            if (type === 'static' && this.options.dropdownFilter === true || typeof this.options.dropdownFilter === 'string') {
                this.dropdownFilter.static.push({
                    key: 'group',
                    template: '{{group}}',
                    all: all,
                    value: Object.keys(this.options.source)
                });
            }

            for (var i = 0, ii = this.dropdownFilter[type].length; i < ii; i++) {

                filter = this.dropdownFilter[type][i];

                if (!Array.isArray(filter.value)) {
                    filter.value = [filter.value];
                }

                if (filter.all) {
                    this.dropdownFilterAll = filter.all;
                }

                for (var k = 0, kk = filter.value.length; k <= kk; k++) {

                    // Only add "all" at the last filter iteration
                    if (k === kk && (i !== ii - 1)) {
                        continue;
                    } else if (k === kk && (i === ii - 1)) {
                        if (type === 'static' && this.dropdownFilter.dynamic.length) {
                            continue;
                        }
                    }

                    template = this.dropdownFilterAll || all;
                    if (filter.value[k]) {
                        if (filter.template) {
                            template = filter.template.replace(new RegExp('\{\{' + filter.key + '}}', 'gi'), filter.value[k]);
                        } else {
                            template = filter.value[k];
                        }
                    } else {
                        this.container.find('.' + scope.options.selector.filterButton).html(template);
                    }

                    (function (k, filter, template) {

                        ulScope.append(
                            $("<li/>", {
                                "class": scope.options.selector.dropdownItem + ' ' + scope.helper.slugify.call(scope, filter.key + '-' + (filter.value[k] || all)),
                                "html": $("<a/>", {
                                    "href": "javascript:;",
                                    "html": template,
                                    "click": function (e) {
                                        e.preventDefault();
                                        _selectFilter.call(scope, {
                                            key: filter.key,
                                            value: filter.value[k] || '*',
                                            template: template
                                        });
                                    }
                                })
                            })
                        );

                    }(k, filter, template));
                }
            }

            if (this.dropdownFilter[type].length) {
                this.container.find('.' + scope.options.selector.filterButton).removeAttr('style');
            }

            /**
             * @private
             * Select the filter and rebuild the result group
             *
             * @param {object} item
             */
            function _selectFilter(item) {
                if (item.value === "*") {
                    delete this.filters.dropdown;
                } else {
                    this.filters.dropdown = item;
                }

                this.container
                    .removeClass('filter')
                    .find('.' + this.options.selector.filterButton)
                    .html(item.template);

                this.isDropdownEvent = true;
                this.node.trigger('dynamic' + this.namespace);

                this.node.focus();
            }
        },

        dynamicFilter: {
            isEnabled: false,
            init: function () {

                if (!this.options.dynamicFilter) return;

                this.dynamicFilter.bind.call(this);
                this.dynamicFilter.isEnabled = true;

            },

            validate: function (item) {

                var isValid,
                    softValid = null,
                    hardValid = null,
                    itemValue;

                for (var key in this.filters.dynamic) {
                    if (!this.filters.dynamic.hasOwnProperty(key)) continue;
                    if (!!~key.indexOf('.')) {
                        itemValue = this.helper.namespace(key, item, 'get');
                    } else {
                        itemValue = item[key];
                    }

                    if (this.filters.dynamic[key].modifier === '|' && !softValid) {
                        softValid = itemValue == this.filters.dynamic[key].value || false;
                    }

                    if (this.filters.dynamic[key].modifier === '&') {
                        // Leaving "==" in case of comparing number with string
                        if (itemValue == this.filters.dynamic[key].value) {
                            hardValid = true;
                        } else {
                            hardValid = false;
                            break;
                        }
                    }
                }

                isValid = softValid;
                if (hardValid !== null) {
                    isValid = hardValid;
                    if (hardValid === true && softValid !== null) {
                        isValid = softValid;
                    }
                }

                return !!isValid;

            },

            set: function (key, value) {

                var matches = key.match(/^([|&])?(.+)/);

                if (!value) {
                    delete this.filters.dynamic[matches[2]];
                } else {
                    this.filters.dynamic[matches[2]] = {
                        modifier: matches[1] || '|',
                        value: value
                    };
                }

                if (this.dynamicFilter.isEnabled) {
                    this.searchResult();
                    this.buildLayout();
                }

            },
            bind: function () {

                var scope = this,
                    filter;

                for (var i = 0, ii = this.options.dynamicFilter.length; i < ii; i++) {

                    filter = this.options.dynamicFilter[i];

                    if (typeof filter.selector === "string") {
                        filter.selector = $(filter.selector);
                    }

                    if (!(filter.selector instanceof $) || !filter.selector[0] || !filter.key) {
                        // {debug}
                        if (this.options.debug) {
                            _debug.log({
                                'node': this.node.selector,
                                'function': 'buildDynamicLayout()',
                                'message': 'Invalid jQuery selector or jQuery Object for "filter.selector" or missing filter.key'
                            });

                            _debug.print();
                        }
                        // {/debug}
                        continue;
                    }

                    (function (filter) {
                        filter.selector.off(scope.namespace).on('change' + scope.namespace, function () {
                            scope.dynamicFilter.set.apply(scope, [filter.key, scope.dynamicFilter.getValue(this)]);
                        }).trigger('change' + scope.namespace);
                    }(filter));

                }
            },

            getValue: function (tag) {
                var value;
                if (tag.tagName === "SELECT") {
                    value = tag.value;
                } else if (tag.tagName === "INPUT") {
                    if (tag.type === "checkbox") {
                        value = tag.checked && tag.getAttribute('value') || tag.checked || null;
                    } else if (tag.type === "radio" && tag.checked) {
                        value = tag.value;
                    }
                }
                return value;
            }
        },

        showLayout: function () {

            // Means the container is already visible
            if (this.container.hasClass('result')) return;

            // Do not add display classes if there are no results
            if (!this.result.length && !this.options.emptyTemplate && !this.options.backdropOnFocus) {
                return;
            }

            var scope = this;

            $('html').off(this.namespace)
                .on("click" + this.namespace + " touchend" + this.namespace, function (e) {
                    if ($(e.target).closest(scope.container)[0] || scope.hasDragged) return;
                    scope.hideLayout();
                });

            this.container.addClass([
                this.result.length || (this.options.emptyTemplate && this.query.length >= this.options.minLength) ? 'result ' : '',
                this.options.hint && this.query.length >= this.options.minLength ? 'hint' : '',
                this.options.backdrop || this.options.backdropOnFocus ? 'backdrop' : ''].join(' ')
            );

            this.helper.executeCallback.call(this, this.options.callback.onShowLayout, [this.node, this.query]);

        },

        hideLayout: function () {

            // Means the container is already hidden
            if (!this.container.hasClass('result') && !this.container.hasClass('backdrop')) return;

            this.container.removeClass('result hint filter' + (this.options.backdropOnFocus && $(this.node).is(':focus') ? '' : ' backdrop'));

            if (this.options.backdropOnFocus && this.container.hasClass('backdrop')) return;

            // Make sure the event gets cleared in case of "ESC"
            $('html').off(this.namespace);

            this.helper.executeCallback.call(this, this.options.callback.onHideLayout, [this.node, this.query]);

        },

        resetLayout: function () {

            this.result = {};
            this.resultCount = 0;
            this.resultCountPerGroup = {};
            this.resultItemCount = 0;
            this.resultHtml = null;

            if (this.options.hint && this.hint.container) {
                this.hint.container.val('');
            }

        },

        resetInput: function () {

            this.node.val('');
            this.item = null;
            this.query = '';
            this.rawQuery = '';

        },

        buildCancelButtonLayout: function () {
            if (!this.options.cancelButton) return;
            var scope = this;

            $('<span/>', {
                "class": this.options.selector.cancelButton,
                "mousedown": function (e) {
                    // Don't blur the input
                    e.stopImmediatePropagation();
                    e.preventDefault();

                    scope.resetInput();
                    scope.node.trigger('input' + scope.namespace, [e]);
                }
            }).insertBefore(this.node);

        },

        toggleCancelButton: function () {
            this.container.toggleClass('cancel', !!this.query.length);
        },

        __construct: function () {
            this.extendOptions();

            if (!this.unifySourceFormat()) {
                return;
            }

            this.dynamicFilter.init.apply(this);

            this.init();
            this.delegateEvents();
            this.buildCancelButtonLayout();
            this.buildDropdownLayout();
            this.buildDropdownItemLayout('static');

            this.helper.executeCallback.call(this, this.options.callback.onReady, [this.node]);
        },

        helper: {

            isEmpty: function (obj) {
                for (var prop in obj) {
                    if (obj.hasOwnProperty(prop))
                        return false;
                }

                return true;
            },

            /**
             * Remove every accent(s) from a string
             *
             * @param {String} string
             * @returns {*}
             */
            removeAccent: function (string) {
                if (typeof string !== "string") {
                    return;
                }

                var accent = _accent;

                if (typeof this.options.accent === "object") {
                    accent = this.options.accent;
                }

                string = string.toLowerCase().replace(new RegExp('[' + accent.from + ']', 'g'), function (match) {
                    return accent.to[accent.from.indexOf(match)];
                });

                return string;
            },

            /**
             * Creates a valid url from string
             *
             * @param {String} string
             * @returns {string}
             */
            slugify: function (string) {

                string = String(string);

                if (string !== "") {
                    string = this.helper.removeAccent.call(this, string);
                    string = string.replace(/[^-a-z0-9]+/g, '-').replace(/-+/g, '-').replace(/^-|-$/g, '');
                }

                return string;
            },

            /**
             * Sort list of object by key
             *
             * @param {String|Array} field
             * @param {Boolean} reverse
             * @param {Function} primer
             * @returns {Function}
             */
            sort: function (field, reverse, primer) {
                var key = function (x) {
                    for (var i = 0, ii = field.length; i < ii; i++) {
                        if (typeof x[field[i]] !== 'undefined') {
                            return primer(x[field[i]]);
                        }
                    }
                    return x;
                };

                reverse = [-1, 1][+!!reverse];

                return function (a, b) {
                    return a = key(a), b = key(b), reverse * ((a > b) - (b > a));
                };
            },

            /**
             * Replace a string from-to index
             *
             * @param {String} string The complete string to replace into
             * @param {Number} offset The cursor position to start replacing from
             * @param {Number} length The length of the replacing string
             * @param {String} replace The replacing string
             * @returns {String}
             */
            replaceAt: function (string, offset, length, replace) {
                return string.substring(0, offset) + replace + string.substring(offset + length);
            },

            /**
             * Adds <strong> html around a matched string
             *
             * @param {String} string The complete string to match from
             * @param {String} key
             * @param {Boolean} [accents]
             * @returns {*}
             */
            highlight: function (string, keys, accents) {

                string = String(string);

                var searchString = accents && this.helper.removeAccent.call(this, string) || string,
                    matches = [];

                if (!Array.isArray(keys)) {
                    keys = [keys];
                }

                keys.sort(function (a, b) {
                    return b.length - a.length;
                });

                // Make sure the '|' join will be safe!
                for (var i = keys.length - 1; i >= 0; i--) {
                    if (keys[i].trim() === "") {
                        keys.splice(i, 1);
                        continue;
                    }
                    keys[i] = keys[i].replace(/[-[\]{}()*+?.,\\^$|#\s]/g, "\\$&");
                }

                searchString.replace(
                    new RegExp('(?:' + keys.join('|') + ')(?!([^<]+)?>)', 'gi'),
                    function (match, index, offset) {
                        matches.push({
                            offset: offset,
                            length: match.length
                        });
                    }
                );

                for (var i = matches.length - 1; i >= 0; i--) {
                    string = this.helper.replaceAt(
                        string,
                        matches[i].offset,
                        matches[i].length,
                        "<strong>" + string.substr(matches[i].offset, matches[i].length) + "</strong>"
                    );
                }

                return string;
            },

            /**
             * Get carret position, mainly used for right arrow navigation
             * @param element
             * @returns {*}
             */
            getCaret: function (element) {
                if (element.selectionStart) {
                    return element.selectionStart;
                } else if (document.selection) {
                    element.focus();

                    var r = document.selection.createRange();
                    if (r === null) {
                        return 0;
                    }

                    var re = element.createTextRange(),
                        rc = re.duplicate();
                    re.moveToBookmark(r.getBookmark());
                    rc.setEndPoint('EndToStart', re);

                    return rc.text.length;
                }
                return 0;
            },

            /**
             * Clean strings from possible XSS (script and iframe tags)
             * @param string
             * @returns {string}
             */
            cleanStringFromScript: function (string) {
                return typeof string === "string" &&
                    string.replace(/<\/?(?:script|iframe)\b[^>]*>/gm, '') ||
                    string;
            },

            /**
             * Executes an anonymous function or a string reached from the window scope.
             *
             * @example
             * Note: These examples works with every configuration callbacks
             *
             * // An anonymous function inside the "onInit" option
             * onInit: function() { console.log(':D'); };
             *
             * // myFunction() located on window.coucou scope
             * onInit: 'window.coucou.myFunction'
             *
             * // myFunction(a,b) located on window.coucou scope passing 2 parameters
             * onInit: ['window.coucou.myFunction', [':D', ':)']];
             *
             * // Anonymous function to execute a local function
             * onInit: function () { myFunction(':D'); }
             *
             * @param {String|Array} callback The function to be called
             * @param {Array} [extraParams] In some cases the function can be called with Extra parameters (onError)
             * @returns {*}
             */
            executeCallback: function (callback, extraParams) {

                if (!callback) {
                    return;
                }

                var _callback;

                if (typeof callback === "function") {

                    _callback = callback;

                } else if (typeof callback === "string" || Array.isArray(callback)) {

                    if (typeof callback === "string") {
                        callback = [callback, []];
                    }

                    _callback = this.helper.namespace(callback[0], window);

                    if (typeof _callback !== "function") {
                        // {debug}
                        if (this.options.debug) {
                            _debug.log({
                                'node': this.selector,
                                'function': 'executeCallback()',
                                'arguments': JSON.stringify(callback),
                                'message': 'WARNING - Invalid callback function"'
                            });

                            _debug.print();
                        }
                        // {/debug}
                        return;
                    }

                }

                return _callback.apply(this, (callback[1] || []).concat(extraParams ? extraParams : []));

            },

            namespace: function (namespaceString, objectReference, method, objectValue) {

                if (typeof namespaceString !== "string" || namespaceString === "") {
                    // {debug}
                    if (this.options.debug) {
                        _debug.log({
                            'node': _node.selector,
                            'function': 'namespace()',
                            'arguments': namespaceString,
                            'message': 'ERROR - Missing namespaceString"'
                        });

                        _debug.print();
                    }
                    // {/debug}
                    return false;
                }

                var parts = namespaceString.split('.'),
                    parent = objectReference || window,
                    method = method || 'get',
                    value = objectValue || {},
                    currentPart = '';

                for (var i = 0, length = parts.length; i < length; i++) {
                    currentPart = parts[i];

                    if (typeof parent[currentPart] === "undefined") {
                        if (~['get', 'delete'].indexOf(method)) {
                            return typeof objectValue !== "undefined" ? objectValue : undefined;
                        }
                        parent[currentPart] = {};
                    }

                    if (~['set', 'create', 'delete'].indexOf(method)) {
                        if (i === length - 1) {
                            if (method === 'set' || method === 'create') {
                                parent[currentPart] = value;
                            } else {

                                delete parent[currentPart];
                                return true;
                            }
                        }
                    }

                    parent = parent[currentPart];

                }
                return parent;
            },

            typeWatch: (function () {
                var timer = 0;
                return function (callback, ms) {
                    clearTimeout(timer);
                    timer = setTimeout(callback, ms);
                };
            })()

        }
    };

    /**
     * @public
     * Implement Typeahead on the selected input node.
     *
     * @param {Object} options
     * @return {Object} Modified DOM element
     */
    $.fn.typeahead = $.typeahead = function (options) {
        return _api.typeahead(this, options);
    };

    /**
     * @private
     * API to handles Typeahead methods via jQuery.
     */
    var _api = {

        /**
         * Enable Typeahead
         *
         * @param {Object} node
         * @param {Object} options
         * @returns {*}
         */
        typeahead: function (node, options) {

            if (!options || !options.source || typeof options.source !== 'object') {

                // {debug}
                _debug.log({
                    'node': node.selector || options && options.input,
                    'function': '$.typeahead()',
                    'arguments': JSON.stringify(options && options.source || ''),
                    'message': 'Undefined "options" or "options.source" or invalid source type - Typeahead dropped'
                });

                _debug.print();
                // {/debug}

                return;
            }

            if (typeof node === "function") {
                if (!options.input) {

                    // {debug}
                    _debug.log({
                        'node': node.selector,
                        'function': '$.typeahead()',
                        //'arguments': JSON.stringify(options),
                        'message': 'Undefined "options.input" - Typeahead dropped'
                    });

                    _debug.print();
                    // {/debug}

                    return;
                }

                node = $(options.input);
            }

            if (!node.length || node[0].nodeName !== "INPUT") {

                // {debug}
                _debug.log({
                    'node': node.selector,
                    'function': '$.typeahead()',
                    'arguments': JSON.stringify(options.input),
                    'message': 'Unable to find jQuery input element - Typeahead dropped'
                });

                _debug.print();
                // {/debug}

                return;
            }

            // Forcing node.selector... damn you jQuery...
            if (options.input && !node.selector) {
                node.selector = options.input;
            }

            /*jshint boss:true */
            return window.Typeahead[options.input || node.selector] = new Typeahead(node, options);

        }

    };

// {debug}
    var _debug = {

        table: {},
        log: function (debugObject) {

            if (!debugObject.message || typeof debugObject.message !== "string") {
                return;
            }

            this.table[debugObject.message] = $.extend({
                'node': '',
                'function': '',
                'arguments': ''
            }, debugObject);

        },
        print: function () {

            if (Typeahead.prototype.helper.isEmpty(this.table) || !console || !console.table) {
                return;
            }

            if (console.group !== undefined || console.table !== undefined) {
                console.groupCollapsed('--- jQuery Typeahead Debug ---');
                console.table(this.table);
                console.groupEnd();
            }

            this.table = {};

        }

    };
    _debug.log({
        'message': 'WARNING - You are using the DEBUG version. Use /dist/jquery.typeahead.min.js in production.'
    });

    _debug.print();
// {/debug}

// IE8 Shims
    window.console = window.console || {
            log: function () {
            }
        };

    if (!Array.isArray) {
        Array.isArray = function (arg) {
            return Object.prototype.toString.call(arg) === '[object Array]';
        };
    }

    if (!('trim' in String.prototype)) {
        String.prototype.trim = function () {
            return this.replace(/^\s+/, '').replace(/\s+$/, '');
        };
    }
    if (!('indexOf' in Array.prototype)) {
        Array.prototype.indexOf = function (find, i /*opt*/) {
            if (i === undefined) i = 0;
            if (i < 0) i += this.length;
            if (i < 0) i = 0;
            for (var n = this.length; i < n; i++)
                if (i in this && this[i] === find)
                    return i;
            return -1;
        };
    }
    if (!Object.keys) {
        Object.keys = function (obj) {
            var keys = [],
                k;
            for (k in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, k)) {
                    keys.push(k);
                }
            }
            return keys;
        };
    }

    return Typeahead;

}));

/////    /////    /////    /////
/////    /////    /////    /////
/////    /////    /////    /////
/////    /////    /////    /////
/////             /////    /////
/////             /////    /////
/////    /////    /////    /////
/////    /////    /////    /////
         /////    /////
         /////    /////
/////    /////    /////    /////
/////    /////    /////    /////
/////    /////    /////    /////
/////    /////    /////    /////

/**
 * ScrollReveal
 * ------------
 * Version : 3.3.2
 * Website : scrollrevealjs.org
 * Repo    : github.com/jlmakes/scrollreveal.js
 * Author  : Julian Lloyd (@jlmakes)
 */

;(function () {
  'use strict'

  var sr
  var _requestAnimationFrame

  function ScrollReveal (config) {
    // Support instantiation without the `new` keyword.
    if (typeof this === 'undefined' || Object.getPrototypeOf(this) !== ScrollReveal.prototype) {
      return new ScrollReveal(config)
    }

    sr = this // Save reference to instance.
    sr.version = '3.3.2'
    sr.tools = new Tools() // *required utilities

    if (sr.isSupported()) {
      sr.tools.extend(sr.defaults, config || {})

      sr.defaults.container = _resolveContainer(sr.defaults)

      sr.store = {
        elements: {},
        containers: []
      }

      sr.sequences = {}
      sr.history = []
      sr.uid = 0
      sr.initialized = false
    } else if (typeof console !== 'undefined' && console !== null) {
      // Note: IE9 only supports console if devtools are open.
      console.log('ScrollReveal is not supported in this browser.')
    }

    return sr
  }

  /**
   * Configuration
   * -------------
   * This object signature can be passed directly to the ScrollReveal constructor,
   * or as the second argument of the `reveal()` method.
   */

  ScrollReveal.prototype.defaults = {
    // 'bottom', 'left', 'top', 'right'
    origin: 'bottom',

    // Can be any valid CSS distance, e.g. '5rem', '10%', '20vw', etc.
    distance: '20px',

    // Time in milliseconds.
    duration: 500,
    delay: 0,

    // Starting angles in degrees, will transition from these values to 0 in all axes.
    rotate: { x: 0, y: 0, z: 0 },

    // Starting opacity value, before transitioning to the computed opacity.
    opacity: 0,

    // Starting scale value, will transition from this value to 1
    scale: 0.9,

    // Accepts any valid CSS easing, e.g. 'ease', 'ease-in-out', 'linear', etc.
    easing: 'cubic-bezier(0.6, 0.2, 0.1, 1)',

    // `<html>` is the default reveal container. You can pass either:
    // DOM Node, e.g. document.querySelector('.fooContainer')
    // Selector, e.g. '.fooContainer'
    container: window.document.documentElement,

    // true/false to control reveal animations on mobile.
    mobile: true,

    // true:  reveals occur every time elements become visible
    // false: reveals occur once as elements become visible
    reset: false,

    // 'always' — delay for all reveal animations
    // 'once'   — delay only the first time reveals occur
    // 'onload' - delay only for animations triggered by first load
    useDelay: 'always',

    // Change when an element is considered in the viewport. The default value
    // of 0.20 means 20% of an element must be visible for its reveal to occur.
    viewFactor: 0.2,

    // Pixel values that alter the container boundaries.
    // e.g. Set `{ top: 48 }`, if you have a 48px tall fixed toolbar.
    // --
    // Visual Aid: https://scrollrevealjs.org/assets/viewoffset.png
    viewOffset: { top: 0, right: 0, bottom: 0, left: 0 },

    // Callbacks that fire for each triggered element reveal, and reset.
    beforeReveal: function (domEl) {},
    beforeReset: function (domEl) {},

    // Callbacks that fire for each completed element reveal, and reset.
    afterReveal: function (domEl) {},
    afterReset: function (domEl) {}
  }

  /**
   * Check if client supports CSS Transform and CSS Transition.
   * @return {boolean}
   */
  ScrollReveal.prototype.isSupported = function () {
    var style = document.documentElement.style
    return 'WebkitTransition' in style && 'WebkitTransform' in style ||
      'transition' in style && 'transform' in style
  }

  /**
   * Creates a reveal set, a group of elements that will animate when they
   * become visible. If [interval] is provided, a new sequence is created
   * that will ensure elements reveal in the order they appear in the DOM.
   *
   * @param {Node|NodeList|string} [target]   The node, node list or selector to use for animation.
   * @param {Object}               [config]   Override the defaults for this reveal set.
   * @param {number}               [interval] Time between sequenced element animations (milliseconds).
   * @param {boolean}              [sync]     Used internally when updating reveals for async content.
   *
   * @return {Object} The current ScrollReveal instance.
   */
  ScrollReveal.prototype.reveal = function (target, config, interval, sync) {
    var container
    var elements
    var elem
    var elemId
    var sequence
    var sequenceId

    // No custom configuration was passed, but a sequence interval instead.
    // let’s shuffle things around to make sure everything works.
    if (config !== undefined && typeof config === 'number') {
      interval = config
      config = {}
    } else if (config === undefined || config === null) {
      config = {}
    }

    container = _resolveContainer(config)
    elements = _getRevealElements(target, container)

    if (!elements.length) {
      console.log('ScrollReveal: reveal on "' + target + '" failed, no elements found.')
      return sr
    }

    // Prepare a new sequence if an interval is passed.
    if (interval && typeof interval === 'number') {
      sequenceId = _nextUid()

      sequence = sr.sequences[sequenceId] = {
        id: sequenceId,
        interval: interval,
        elemIds: [],
        active: false
      }
    }

    // Begin main loop to configure ScrollReveal elements.
    for (var i = 0; i < elements.length; i++) {
      // Check if the element has already been configured and grab it from the store.
      elemId = elements[i].getAttribute('data-sr-id')
      if (elemId) {
        elem = sr.store.elements[elemId]
      } else {
        // Otherwise, let’s do some basic setup.
        elem = {
          id: _nextUid(),
          domEl: elements[i],
          seen: false,
          revealing: false
        }
        elem.domEl.setAttribute('data-sr-id', elem.id)
      }

      // Sequence only setup
      if (sequence) {
        elem.sequence = {
          id: sequence.id,
          index: sequence.elemIds.length
        }

        sequence.elemIds.push(elem.id)
      }

      // New or existing element, it’s time to update its configuration, styles,
      // and send the updates to our store.
      _configure(elem, config, container)
      _style(elem)
      _updateStore(elem)

      // We need to make sure elements are set to visibility: visible, even when
      // on mobile and `config.mobile === false`, or if unsupported.
      if (sr.tools.isMobile() && !elem.config.mobile || !sr.isSupported()) {
        elem.domEl.setAttribute('style', elem.styles.inline)
        elem.disabled = true
      } else if (!elem.revealing) {
        // Otherwise, proceed normally.
        elem.domEl.setAttribute('style',
          elem.styles.inline +
          elem.styles.transform.initial
        )
      }
    }

    // Each `reveal()` is recorded so that when calling `sync()` while working
    // with asynchronously loaded content, it can re-trace your steps but with
    // all your new elements now in the DOM.

    // Since `reveal()` is called internally by `sync()`, we don’t want to
    // record or intiialize each reveal during syncing.
    if (!sync && sr.isSupported()) {
      _record(target, config, interval)

      // We push initialization to the event queue using setTimeout, so that we can
      // give ScrollReveal room to process all reveal calls before putting things into motion.
      // --
      // Philip Roberts - What the heck is the event loop anyway? (JSConf EU 2014)
      // https://www.youtube.com/watch?v=8aGhZQkoFbQ
      if (sr.initTimeout) {
        window.clearTimeout(sr.initTimeout)
      }
      sr.initTimeout = window.setTimeout(_init, 0)
    }

    return sr
  }

  /**
   * Re-runs `reveal()` for each record stored in history, effectively capturing
   * any content loaded asynchronously that matches existing reveal set targets.
   * @return {Object} The current ScrollReveal instance.
   */
  ScrollReveal.prototype.sync = function () {
    if (sr.history.length && sr.isSupported()) {
      for (var i = 0; i < sr.history.length; i++) {
        var record = sr.history[i]
        sr.reveal(record.target, record.config, record.interval, true)
      }
      _init()
    } else {
      console.log('ScrollReveal: sync failed, no reveals found.')
    }
    return sr
  }

  /**
   * Private Methods
   * ---------------
   */

  function _resolveContainer (config) {
    if (config && config.container) {
      if (typeof config.container === 'string') {
        return window.document.documentElement.querySelector(config.container)
      } else if (sr.tools.isNode(config.container)) {
        return config.container
      } else {
        console.log('ScrollReveal: invalid container "' + config.container + '" provided.')
        console.log('ScrollReveal: falling back to default container.')
      }
    }
    return sr.defaults.container
  }

  /**
   * check to see if a node or node list was passed in as the target,
   * otherwise query the container using target as a selector.
   *
   * @param {Node|NodeList|string} [target]    client input for reveal target.
   * @param {Node}                 [container] parent element for selector queries.
   *
   * @return {array} elements to be revealed.
   */
  function _getRevealElements (target, container) {
    if (typeof target === 'string') {
      return Array.prototype.slice.call(container.querySelectorAll(target))
    } else if (sr.tools.isNode(target)) {
      return [target]
    } else if (sr.tools.isNodeList(target)) {
      return Array.prototype.slice.call(target)
    }
    return []
  }

  /**
   * A consistent way of creating unique IDs.
   * @returns {number}
   */
  function _nextUid () {
    return ++sr.uid
  }

  function _configure (elem, config, container) {
    // If a container was passed as a part of the config object,
    // let’s overwrite it with the resolved container passed in.
    if (config.container) config.container = container
    // If the element hasn’t already been configured, let’s use a clone of the
    // defaults extended by the configuration passed as the second argument.
    if (!elem.config) {
      elem.config = sr.tools.extendClone(sr.defaults, config)
    } else {
      // Otherwise, let’s use a clone of the existing element configuration extended
      // by the configuration passed as the second argument.
      elem.config = sr.tools.extendClone(elem.config, config)
    }

    // Infer CSS Transform axis from origin string.
    if (elem.config.origin === 'top' || elem.config.origin === 'bottom') {
      elem.config.axis = 'Y'
    } else {
      elem.config.axis = 'X'
    }
  }

  function _style (elem) {
    var computed = window.getComputedStyle(elem.domEl)

    if (!elem.styles) {
      elem.styles = {
        transition: {},
        transform: {},
        computed: {}
      }

      // Capture any existing inline styles, and add our visibility override.
      // --
      // See section 4.2. in the Documentation:
      // https://github.com/jlmakes/scrollreveal.js#42-improve-user-experience
      elem.styles.inline = elem.domEl.getAttribute('style') || ''
      elem.styles.inline += '; visibility: visible; '

      // grab the elements existing opacity.
      elem.styles.computed.opacity = computed.opacity

      // grab the elements existing transitions.
      if (!computed.transition || computed.transition === 'all 0s ease 0s') {
        elem.styles.computed.transition = ''
      } else {
        elem.styles.computed.transition = computed.transition + ', '
      }
    }

    // Create transition styles
    elem.styles.transition.instant = _generateTransition(elem, 0)
    elem.styles.transition.delayed = _generateTransition(elem, elem.config.delay)

    // Generate transform styles, first with the webkit prefix.
    elem.styles.transform.initial = ' -webkit-transform:'
    elem.styles.transform.target = ' -webkit-transform:'
    _generateTransform(elem)

    // And again without any prefix.
    elem.styles.transform.initial += 'transform:'
    elem.styles.transform.target += 'transform:'
    _generateTransform(elem)
  }

  function _generateTransition (elem, delay) {
    var config = elem.config

    return '-webkit-transition: ' + elem.styles.computed.transition +
      '-webkit-transform ' + config.duration / 1000 + 's ' +
      config.easing + ' ' +
      delay / 1000 + 's, opacity ' +
      config.duration / 1000 + 's ' +
      config.easing + ' ' +
      delay / 1000 + 's; ' +

      'transition: ' + elem.styles.computed.transition +
      'transform ' + config.duration / 1000 + 's ' +
      config.easing + ' ' +
      delay / 1000 + 's, opacity ' +
      config.duration / 1000 + 's ' +
      config.easing + ' ' +
      delay / 1000 + 's; '
  }

  function _generateTransform (elem) {
    var config = elem.config
    var cssDistance
    var transform = elem.styles.transform

    // Let’s make sure our our pixel distances are negative for top and left.
    // e.g. origin = 'top' and distance = '25px' starts at `top: -25px` in CSS.
    if (config.origin === 'top' || config.origin === 'left') {
      cssDistance = /^-/.test(config.distance)
        ? config.distance.substr(1)
        : '-' + config.distance
    } else {
      cssDistance = config.distance
    }

    if (parseInt(config.distance)) {
      transform.initial += ' translate' + config.axis + '(' + cssDistance + ')'
      transform.target += ' translate' + config.axis + '(0)'
    }
    if (config.scale) {
      transform.initial += ' scale(' + config.scale + ')'
      transform.target += ' scale(1)'
    }
    if (config.rotate.x) {
      transform.initial += ' rotateX(' + config.rotate.x + 'deg)'
      transform.target += ' rotateX(0)'
    }
    if (config.rotate.y) {
      transform.initial += ' rotateY(' + config.rotate.y + 'deg)'
      transform.target += ' rotateY(0)'
    }
    if (config.rotate.z) {
      transform.initial += ' rotateZ(' + config.rotate.z + 'deg)'
      transform.target += ' rotateZ(0)'
    }
    transform.initial += '; opacity: ' + config.opacity + ';'
    transform.target += '; opacity: ' + elem.styles.computed.opacity + ';'
  }

  function _updateStore (elem) {
    var container = elem.config.container

    // If this element’s container isn’t already in the store, let’s add it.
    if (container && sr.store.containers.indexOf(container) === -1) {
      sr.store.containers.push(elem.config.container)
    }

    // Update the element stored with our new element.
    sr.store.elements[elem.id] = elem
  }

  function _record (target, config, interval) {
    // Save the `reveal()` arguments that triggered this `_record()` call, so we
    // can re-trace our steps when calling the `sync()` method.
    var record = {
      target: target,
      config: config,
      interval: interval
    }
    sr.history.push(record)
  }

  function _init () {
    if (sr.isSupported()) {
      // Initial animate call triggers valid reveal animations on first load.
      // Subsequent animate calls are made inside the event handler.
      _animate()

      // Then we loop through all container nodes in the store and bind event
      // listeners to each.
      for (var i = 0; i < sr.store.containers.length; i++) {
        sr.store.containers[i].addEventListener('scroll', _handler)
        sr.store.containers[i].addEventListener('resize', _handler)
      }

      // Let’s also do a one-time binding of window event listeners.
      if (!sr.initialized) {
        window.addEventListener('scroll', _handler)
        window.addEventListener('resize', _handler)
        sr.initialized = true
      }
    }
    return sr
  }

  function _handler () {
    _requestAnimationFrame(_animate)
  }

  function _setActiveSequences () {
    var active
    var elem
    var elemId
    var sequence

    // Loop through all sequences
    sr.tools.forOwn(sr.sequences, function (sequenceId) {
      sequence = sr.sequences[sequenceId]
      active = false

      // For each sequenced elemenet, let’s check visibility and if
      // any are visible, set it’s sequence to active.
      for (var i = 0; i < sequence.elemIds.length; i++) {
        elemId = sequence.elemIds[i]
        elem = sr.store.elements[elemId]
        if (_isElemVisible(elem) && !active) {
          active = true
        }
      }

      sequence.active = active
    })
  }

  function _animate () {
    var delayed
    var elem

    _setActiveSequences()

    // Loop through all elements in the store
    sr.tools.forOwn(sr.store.elements, function (elemId) {
      elem = sr.store.elements[elemId]
      delayed = _shouldUseDelay(elem)

      // Let’s see if we should revealand if so,
      // trigger the `beforeReveal` callback and
      // determine whether or not to use delay.
      if (_shouldReveal(elem)) {
        elem.config.beforeReveal(elem.domEl)
        if (delayed) {
          elem.domEl.setAttribute('style',
            elem.styles.inline +
            elem.styles.transform.target +
            elem.styles.transition.delayed
          )
        } else {
          elem.domEl.setAttribute('style',
            elem.styles.inline +
            elem.styles.transform.target +
            elem.styles.transition.instant
          )
        }

        // Let’s queue the `afterReveal` callback
        // and mark the element as seen and revealing.
        _queueCallback('reveal', elem, delayed)
        elem.revealing = true
        elem.seen = true

        if (elem.sequence) {
          _queueNextInSequence(elem, delayed)
        }
      } else if (_shouldReset(elem)) {
        //Otherwise reset our element and
        // trigger the `beforeReset` callback.
        elem.config.beforeReset(elem.domEl)
        elem.domEl.setAttribute('style',
          elem.styles.inline +
          elem.styles.transform.initial +
          elem.styles.transition.instant
        )
        // And queue the `afterReset` callback.
        _queueCallback('reset', elem)
        elem.revealing = false
      }
    })
  }

  function _queueNextInSequence (elem, delayed) {
    var elapsed = 0
    var delay = 0
    var sequence = sr.sequences[elem.sequence.id]

    // We’re processing a sequenced element, so let's block other elements in this sequence.
    sequence.blocked = true

    // Since we’re triggering animations a part of a sequence after animations on first load,
    // we need to check for that condition and explicitly add the delay to our timer.
    if (delayed && elem.config.useDelay === 'onload') {
      delay = elem.config.delay
    }

    // If a sequence timer is already running, capture the elapsed time and clear it.
    if (elem.sequence.timer) {
      elapsed = Math.abs(elem.sequence.timer.started - new Date())
      window.clearTimeout(elem.sequence.timer)
    }

    // Start a new timer.
    elem.sequence.timer = { started: new Date() }
    elem.sequence.timer.clock = window.setTimeout(function () {
      // Sequence interval has passed, so unblock the sequence and re-run the handler.
      sequence.blocked = false
      elem.sequence.timer = null
      _handler()
    }, Math.abs(sequence.interval) + delay - elapsed)
  }

  function _queueCallback (type, elem, delayed) {
    var elapsed = 0
    var duration = 0
    var callback = 'after'

    // Check which callback we’re working with.
    switch (type) {
      case 'reveal':
        duration = elem.config.duration
        if (delayed) {
          duration += elem.config.delay
        }
        callback += 'Reveal'
        break

      case 'reset':
        duration = elem.config.duration
        callback += 'Reset'
        break
    }

    // If a timer is already running, capture the elapsed time and clear it.
    if (elem.timer) {
      elapsed = Math.abs(elem.timer.started - new Date())
      window.clearTimeout(elem.timer.clock)
    }

    // Start a new timer.
    elem.timer = { started: new Date() }
    elem.timer.clock = window.setTimeout(function () {
      // The timer completed, so let’s fire the callback and null the timer.
      elem.config[callback](elem.domEl)
      elem.timer = null
    }, duration - elapsed)
  }

  function _shouldReveal (elem) {
    if (elem.sequence) {
      var sequence = sr.sequences[elem.sequence.id]
      return sequence.active &&
        !sequence.blocked &&
        !elem.revealing &&
        !elem.disabled
    }
    return _isElemVisible(elem) &&
      !elem.revealing &&
      !elem.disabled
  }

  function _shouldUseDelay (elem) {
    var config = elem.config.useDelay
    return config === 'always' ||
      (config === 'onload' && !sr.initialized) ||
      (config === 'once' && !elem.seen)
  }

  function _shouldReset (elem) {
    if (elem.sequence) {
      var sequence = sr.sequences[elem.sequence.id]
      return !sequence.active &&
        elem.config.reset &&
        elem.revealing &&
        !elem.disabled
    }
    return !_isElemVisible(elem) &&
      elem.config.reset &&
      elem.revealing &&
      !elem.disabled
  }

  function _getContainer (container) {
    return {
      width: container.clientWidth,
      height: container.clientHeight
    }
  }

  function _getScrolled (container) {
    // Return the container scroll values, plus the its offset.
    if (container && container !== window.document.documentElement) {
      var offset = _getOffset(container)
      return {
        x: container.scrollLeft + offset.left,
        y: container.scrollTop + offset.top
      }
    } else {
      // Otherwise, default to the window object’s scroll values.
      return {
        x: window.pageXOffset,
        y: window.pageYOffset
      }
    }
  }

  function _getOffset (domEl) {
    var offsetTop = 0
    var offsetLeft = 0

      // Grab the element’s dimensions.
    var offsetHeight = domEl.offsetHeight
    var offsetWidth = domEl.offsetWidth

    // Now calculate the distance between the element and its parent, then
    // again for the parent to its parent, and again etc... until we have the
    // total distance of the element to the document’s top and left origin.
    do {
      if (!isNaN(domEl.offsetTop)) {
        offsetTop += domEl.offsetTop
      }
      if (!isNaN(domEl.offsetLeft)) {
        offsetLeft += domEl.offsetLeft
      }
      domEl = domEl.offsetParent
    } while (domEl)

    return {
      top: offsetTop,
      left: offsetLeft,
      height: offsetHeight,
      width: offsetWidth
    }
  }

  function _isElemVisible (elem) {
    var offset = _getOffset(elem.domEl)
    var container = _getContainer(elem.config.container)
    var scrolled = _getScrolled(elem.config.container)
    var vF = elem.config.viewFactor

      // Define the element geometry.
    var elemHeight = offset.height
    var elemWidth = offset.width
    var elemTop = offset.top
    var elemLeft = offset.left
    var elemBottom = elemTop + elemHeight
    var elemRight = elemLeft + elemWidth

    return confirmBounds() || isPositionFixed()

    function confirmBounds () {
      // Define the element’s functional boundaries using its view factor.
      var top = elemTop + elemHeight * vF
      var left = elemLeft + elemWidth * vF
      var bottom = elemBottom - elemHeight * vF
      var right = elemRight - elemWidth * vF

      // Define the container functional boundaries using its view offset.
      var viewTop = scrolled.y + elem.config.viewOffset.top
      var viewLeft = scrolled.x + elem.config.viewOffset.left
      var viewBottom = scrolled.y - elem.config.viewOffset.bottom + container.height
      var viewRight = scrolled.x - elem.config.viewOffset.right + container.width

      return top < viewBottom &&
        bottom > viewTop &&
        left > viewLeft &&
        right < viewRight
    }

    function isPositionFixed () {
      return (window.getComputedStyle(elem.domEl).position === 'fixed')
    }
  }

  /**
   * Utilities
   * ---------
   */

  function Tools () {}

  Tools.prototype.isObject = function (object) {
    return object !== null && typeof object === 'object' && object.constructor === Object
  }

  Tools.prototype.isNode = function (object) {
    return typeof window.Node === 'object'
      ? object instanceof window.Node
      : object && typeof object === 'object' &&
        typeof object.nodeType === 'number' &&
        typeof object.nodeName === 'string'
  }

  Tools.prototype.isNodeList = function (object) {
    var prototypeToString = Object.prototype.toString.call(object)
    var regex = /^\[object (HTMLCollection|NodeList|Object)\]$/

    return typeof window.NodeList === 'object'
      ? object instanceof window.NodeList
      : object && typeof object === 'object' &&
        regex.test(prototypeToString) &&
        typeof object.length === 'number' &&
        (object.length === 0 || this.isNode(object[0]))
  }

  Tools.prototype.forOwn = function (object, callback) {
    if (!this.isObject(object)) {
      throw new TypeError('Expected "object", but received "' + typeof object + '".')
    } else {
      for (var property in object) {
        if (object.hasOwnProperty(property)) {
          callback(property)
        }
      }
    }
  }

  Tools.prototype.extend = function (target, source) {
    this.forOwn(source, function (property) {
      if (this.isObject(source[property])) {
        if (!target[property] || !this.isObject(target[property])) {
          target[property] = {}
        }
        this.extend(target[property], source[property])
      } else {
        target[property] = source[property]
      }
    }.bind(this))
    return target
  }

  Tools.prototype.extendClone = function (target, source) {
    return this.extend(this.extend({}, target), source)
  }

  Tools.prototype.isMobile = function () {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
  }

  /**
   * Polyfills
   * --------
   */

  _requestAnimationFrame = window.requestAnimationFrame ||
    window.webkitRequestAnimationFrame ||
    window.mozRequestAnimationFrame ||
    function (callback) {
      window.setTimeout(callback, 1000 / 60)
    }

  /**
   * Module Wrapper
   * --------------
   */
  if (typeof define === 'function' && typeof define.amd === 'object' && define.amd) {
    define(function () {
      return ScrollReveal
    })
  } else if (typeof module !== 'undefined' && module.exports) {
    module.exports = ScrollReveal
  } else {
    window.ScrollReveal = ScrollReveal
  }
}())

var sidenavTimer = null;
var sidenavState = null;

function openSidenav(delay) {

    if ($(".test").is(':visible')) {
        return;
    }

    sidenavTimer = setTimeout(function () {
        var nav = $("#global-sidenav");
        nav.addClass("open");
        console.log('open');
        $(".darken").fadeIn();
        $(".top-nav").addClass("sidenav-open");
        //$("#search-box").focus();
    }, delay);
}

function closeSidenav() {

    if ($(".test").is(':visible')) {
        return;
    }
    
    console.log('close');

    clearTimeout(sidenavTimer);

    var nav = $("#global-sidenav");
    nav.removeClass("open");

    $(".darken").fadeOut();
    
    $(".account").hide();
    $(".top-nav").removeClass("sidenav-open");

}

function toggleSidenav(delay) {
    var nav = $("#global-sidenav");

    

    if (nav.width() > 200) {
        console.log('closing');
        closeSidenav();
    }
    else {
        console.log('opening');
        openSidenav(delay);
    }
}

$(function () {

    $(document).keyup(function (e) {
        if (e.keyCode == 27) { // escape key maps to keycode `27`
            closeSidenav();

            $('.typeahead').typeahead('setQuery', '');
        }
    });

    $("#global-sidenav").on("mouseenter", function () {
        openSidenav(225);
    })

    $("#global-sidenav").on("mouseleave", function () {
        closeSidenav();
    })

    $("#sidenav-toggler").click(function (e) {
        e.preventDefault();
        toggleSidenav(0);
        return false;
    })

    $(".darken").click(function () {
        closeSidenav();
    });


    $(".toggle").on("click", function (e) {
        e.preventDefault();

        var target = $(this).attr("href");
        var group = $(this).data("group");
        var isVisible = $(target).is(':visible');

        if (isVisible) {
            $(target).hide();
        }
        else {
            $("." + group).hide();
            $(target).show();
        }
        return false;
        
    })


    var lastSize = findBootstrapEnvironment();
    console.log(lastSize);

    window.sr = ScrollReveal();
    sr.reveal('.reveal', { distance: "5px", origin: "right", duration: 500, delay: 0 });
    sr.reveal('.reveal-no-mobile', { distance: "5px", origin: "right", duration: 500, delay: 0, mobile: false });
    sr.reveal('.reveal-slow', { distance: "5px", origin: "right", duration: 2000, delay: 0 });

    $(".scroll-to").click(function (e) {
        e.preventDefault();
        var href = $(this).attr("href");

        $('html, body').animate({
            scrollTop: $(href).offset().top
        }, 1000);
    });

    var sidebar = $('.sidebar-fixed');
    console.log(sidebar);

    if (sidebar.innerWidth() !== undefined) {
        var top = sidebar.offset().top - parseFloat(sidebar.css('margin-top'));
        var sideBarWidth = 0;

        $(window).scroll(function (event) {
            sidebarPosition();
        });
    }

    if ($(window).scrollTop()) {
        sidebarPosition();
    }


    /* off-canvas sidebar toggle */
    $('[data-toggle=offcanvas]').click(function () {
        $('.row-offcanvas').toggleClass('active');
        $('.collapse').toggleClass('in').toggleClass('hidden-xs').toggleClass('visible-xs');
    });


    $(".card-menu .dropdown-item").click(function (e) {
        alert("Demo feature. Not ready yet.");
    });

    $("body").on("mouseenter", ".lineitem-cell", function (e) {
        $(this).find(".trending-percent").fadeIn(500);
    });

    $("body").on("mouseleave", ".lineitem-cell", function (e) {
        $(this).find(".trending-percent").fadeOut();
    });

    $("body").on("show.bs.popover", function (e) {
        
    });



    $("body").on("click", ".inline-report .close", function (e) {
        var target = $(e.currentTarget).parents(".inline-report");
        target.remove();
    });

    $(".mdrm-row").on("click", function (e) {

        var row = $(e.currentTarget);

        var url = row.data("url").toLowerCase();
        var organizationCells = row.parents("table").find("th[data-organization]");

        var organizations = [];

        $.each(organizationCells, function (index, value) {
            organizations.push($(value).data("organization"));
        });

        if (!row.next().hasClass("inline-report")) {

            var newRow = $("<tr style='display:none;' class='inline-report'><td class='no-border' colspan='20'><div class='inside'><div class='p-1 text-xs-center'><i class='fa fa-circle-o-notch fa-spin fa-3x fa-fw'></i></div></div></td></tr>").insertAfter($(e.currentTarget));
            var inside = newRow.find(".inside");

            newRow.show("slow");

            var jqxhr = $.ajax({
                url: url,
                method: "GET",
                data: {
                    c: organizations.join(",")
                }
            })
              .done(function (data) {

                  inside.html(data);

              })
        }
        //else {
        //    console.log(row.next(".inline-report"));
        //    row.next().show(".inline-report");
        //}
    });

    
    
    

    //$('[data-toggle="popover"]').popover({

    //    trigger: 'click',
    //    placement: "top",
    //    html: true,
    //    content: '<div class="fact-options">' +
    //                    '<i class="fa fa-line-chart" aria-hidden="true"></i>' +
    //                    '<i class="fa fa-star-o" aria-hidden="true"></i>' +
    //                    //'<i class="fa fa-bell-o" aria-hidden="true"></i>' +
    //                '</div>'
    //})



    //$("#modal").on("show.bs.modal", function (e) {
    //    var content = $(e.relatedTarget).data("content");
    //    var html = $(e.relatedTarget).data("html");
    //    var modalBody = $(".modal-body");

    //    if (content !== undefined) {

    //        var jqxhr = $.ajax(content)
    //                          .done(function (data) {
    //                              modalBody.html(data);
    //                          })
    //                          .fail(function () {
    //                              console.log("error");
    //                          })
    //                          .always(function () {

    //                          });
    //    }
    //    else if (html !== undefined) {
    //        modalBody.html(html);
    //    }

    //});

    function sidebarPosition() {
        if (lastSize !== "lg") {
            return;
        }

        var y = $(window).scrollTop();
        if (y >= top) {
            sideBarWidth = sidebar.width();
            sidebar.width(sideBarWidth);
            sidebar.addClass('fixed');
            sidebar.find(".sidebar-fixed-hidden").show("slow");
            sidebar.find(".sidebar-fixed-displayed").hide("slow");
        } else {
            sidebar.removeClass('fixed');
            if ($(document).height() > $(window).height()) {
                //if (y === 0) {
                sidebar.find(".sidebar-fixed-hidden").hide("slow");
                sidebar.find(".sidebar-fixed-displayed").show("slow");

            }
        }
    }

    function findBootstrapEnvironment() {
        var envs = ["ExtraSmall", "Small", "Medium", "Large"];
        var envValues = ["xs", "sm", "md", "lg"];

        var $el = $('<div>');
        $el.appendTo($('body'));

        for (var i = envValues.length - 1; i >= 0; i--) {
            var envVal = envValues[i];

            $el.addClass('hidden-' + envVal + "-up");
            if ($el.is(':hidden')) {
                $el.remove();

                return envValues[i];
            }
        };
    }


});


$(function () {


    $.typeahead({
        input: '.js-typeahead-name',
        filter: false,
        dynamic: true,
        minLength: 0,
        searchOnFocus: true,
        delay: 250,
        maxItem: 20,
        //order: "desc",
        template: function (query, item) {
         //   <a href="@Model.Organization.ProfileUrl"
         //   class="preload avatar avatar-md media-object img-responsive"
         //   style="background-image: url('@Model.Organization.Avatar')">

         //</a>
            return '<div class="search-result"><div class="media"><i class="media-left"><a href="{{url}}" class="media-object avatar avatar-sm img-responsive" style="background-image: url({{avatar}})"></a></i><div class="name">{{name}}<br/><small>{{city}}, {{state}} <div class="float-right"><b>{{assets}} Assets</b></div></small></div></div>';
        },
        emptyTemplate: "No results for {{query}}",
        href: "{{url}}",
        source: {
            lenders: {
                display: "name",
                ajax: function (query) {
                    return {
                        url: "/modals/search/data",
                        path: "data.snippets",
                        data: {
                            q: "{{query}}"
                        }
                    }
                }
            }
        },
        callback: {
            onReady: function () {

                //setTimeout(function () {
                //    $("#search-box").focus();
                //}, 750);
            },
            onClick: function (node, a, item, event) {
                window.location = item.url;
            },
            onSubmit: function (node, form, item, event) {
                return false;
            }
        }
    });
});


$(function () {


    Highcharts.theme = {
        colors: ['#058DC7', '#64E572', '#FF9655', '#FFF263', '#6AF9C4', '#50B432', '#ED561B', '#DDDF00'],
        chart: {
            backgroundColor: null,
            borderWidth: 0,
            //margin: [0, 0, 0, 0],
            //marginLeft: 0,
            //marginRight: 0,
            //marginTop: 0,
            //marginBottom: 0,
            //spacingBottom: 0,

            style: {
                overflow: 'visible'
            },
            skipClone: true
        },
        lang: {
            thousandsSep: ","
        },
        title: {
            text: ''
        },
        credits: {
            enabled: false
        },
        xAxis: {
            lineWidth: 0,
            gridLineWidth: 0,
            gridLineColor: '#eeeeee',
            type: "datetime",
            labels: {
                enabled: false
            },
            title: {
                text: null
            },
            startOnTick: false,
            endOnTick: false,
            maxPadding: 0,
            minPadding: 0,
            tickPositions: []
        },
        yAxis: {
            gridLineWidth: 0,
            gridLineColor: '#eeeeee',
            endOnTick: false,
            startOnTick: false,
            labels: {
                enabled: false
            },
            title: {
                text: null
            },
            tickPositions: []
        },
        legend: {
            enabled: true,
            layout: "vertical"
        },
        //tooltip: {
        //    shared: true
        //},
        tooltip: {

            formatter: function () {

                var name = this.series.name;
                var key = this.key;

                if ($.isNumeric(key)) {
                    key = "";
                }

                return '<b>' + name + '</b><br/><i>' + key + "</i><br/>" +
                    Highcharts.dateFormat('%e-%b-%Y', new Date(this.x)) + '<br/> Value: ' + Highcharts.numberFormat(this.y, 0, "", ",") + ' ';
            }
        },
        plotOptions: {
            pie: {
                dataLabels: {
                    enabled: false
                },
                showInLegend: true
            },
            areaspline: {
                pointPlacement: null

            },
            column: {
                pointPadding: 0
            },
            line: {

            },
            series: {
                enabledMouseTracking: false,
                animation: {
                    duration: 1750
                },
                lineWidth: 2,
                //lineColor: "transparent",
                shadow: false,
                states: {
                    hover: {
                        lineWidth: 2
                    }
                },
                marker: {
                    radius: 0,
                    symbol: "circle",
                    states: {
                        hover: {
                            radius: 5
                        }
                    }
                },
                fillOpacity: 0.25
            }
        }
    };

    Highcharts.setOptions(Highcharts.theme);

    var lastHover = null;
    var pending = false;

    $("body").on("mouseenter", ".lineitem-cell,.lineitem-sparkline", function (e) {

        if (pending) {
            return;
        }

        lastHover = this;
        var thisHover = this;

        pending = true;

        setTimeout(function () {


            if (lastHover === thisHover) {

                //console.log('starting timer');
                //console.log(thisHover);
                //console.log(lastHover);

                if ($(thisHover).find(".lineitem-sparkline").exists()) {
                    $(thisHover).find(".lineitem-sparkline").fadeIn();
                    return;
                }

                var series = $(thisHover).data('series');

                var chartDiv = document.createElement("div");
                chartDiv.className = "lineitem-sparkline";
                $(chartDiv).data("series", series);
                $(thisHover).prepend(chartDiv);
                
                
                $(chartDiv).highcharts('SparkLine', {
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 750
                            }
                        }
                    }
                });

            }
        }, 200);



    });

    $("body").on("mouseleave", ".lineitem-cell", function (e) {
        pending = false;
        var that = this;
        setTimeout(function () {
            $(that).find(".lineitem-sparkline").fadeOut();
        }, 500);
    });

});

$(function () {


    $("[data-chart-type='sankey']").each(function (index, element) {

        google.charts.load('current', { 'packages': ['sankey'] });
        google.charts.setOnLoadCallback(function () {
            drawSankeyChart(element);
        });

    });

    $("[data-chart-type='combo']").each(function (index, element) {

        $(this).highcharts('Combo', {

        });

    });

    $("[data-chart-type='key-ratio']").each(function (index, element) {

        $(this).highcharts('Combo', {
            chart: {
                marginBottom: 75,
                spacingBottom: 0
            },
            plotOptions: {
                areaspline: {
                    lineWidth: 0
                }
            },
            legend: {
                enabled: true,
                layout: "horizontal"
            },
            tooltip: {
                //shared: true,
                split: true,
                useHTML: true,
                padding: 0,
                valueDecimals: 2,
                borderWidth: 0,
                headerFormat: "<table class='table'>",
                pointFormat: "<tr><td><b>${point.y}    </b></td><td> {point.x:%b %Y}</td></tr>",
                footerFormat: "</table>",
                //footerFormat: "<b>${point.y}</b> {point.x:%b %Y}",
                formatter: null,
                positioner: function () {
                    return { x: -5, y: -100 };
                },
            },
            series: [
                {},
                { lineWidth: 0 }
            ]
        });

    });


    $("[data-chart-type='fixed-placement']").each(function (index, element) {

        $(this).highcharts('Combo', {
            plotOptions: {
                bar: {
                    grouping: true,
                    borderWidth: 0,
                    groupPadding: 0,
                    pointPadding: 0
                }
            },
            yAxis: {
                max: 100
            },
            series: [
                {
                    color: 'rgba(165,170,217,.5)',
                    pointPadding: 0.1,
                    zIndex: 100
                },
                {
                    color: 'rgba(126,86,134,.9)',
                    pointPadding: 0.2,
                },
                {
                    color: 'rgba(186,60,61,.9)',
                    pointPadding: 0.3,
                }
            ]
        });

    });

    $('[data-chart-type="primary"]').highcharts('Combo', {
        chart: {
            marginBottom: 85,
            spacingBottom: 0
        },
        plotOptions: {
            areaspline: {
                lineWidth: 0,
            }
        },
        lang: {
            thousandsSep: ','
        },
        legend: {
            enabled: true,
            layout: "horizontal"
        },
        tooltip: {
            shared: true,
            //split: true,
            borderWidth: 0,
            shadow: false,
            useHTML: true,
            valueDecimals: 0,
            headerFormat: "<table class='primary-tooltip table table-sm table-striped'><tr><th colspan='2'>{point.x:%b %e %Y}</th></th>",
            pointFormat: "<tr><td style='border-left: 10px solid {point.series.color}'>{point.name}</td><td style='text-align: right;'>{point.y}</td></tr>",
            footerFormat: "</table>",
            formatter: null,
            positioner: function () {
                return { x: 0, y: 0 };
            },
        },
        xAxis: {
            gridLineWidth: 1,
            tickPositions: null,
            labels: {
                enabled: true
            },
        },
        yAxis: {
            gridLineWidth: 0,
            tickPositions: null
        },
        series: [
            {
                fillColor: {
                    linearGradient: { x1: .2, x2: 0, y1: 0, y2: .75 },

                    stops: [
                        [0, Highcharts.Color('#2E96EA').setOpacity(.70).get('rgba')],
                        [1, Highcharts.Color('#30C8CA').setOpacity(.70).get('rgba')]
                    ]
                }
            }
        ]
    });


    function drawSankeyChart(element) {
        var d = $(element).data("series")

        console.log(d);

        var data = google.visualization.arrayToDataTable(d[0].data);

        var view = new google.visualization.DataView(data);

        var max = google.visualization.data.max(view.getDistinctValues(2));

        var filteredRows = view.getFilteredRows([{ column: 2, minValue: max * 0.008 }]);

        view.setRows(filteredRows);

        var colors = ['#a6cee3', '#b2df8a', '#fb9a99', '#fdbf6f',
              '#cab2d6', '#ffff99', '#1f78b4', '#33a02c'];

        // Sets chart options.
        var options = {
            //width: 600,
            tooltip: {
                isHtml: true
            },
            sankey: {
                iterations: 128,
                node: {
                    interactivity: true,
                    nodePadding: 15,
                    width: 15,
                    colors: colors,
                    label: {
                        fontSize: 12
                    }
                },
                link: {
                    colorMode: 'source',
                    colors: colors
                }
            }
        };

        // Instantiates and draws our chart, passing in some options.
        var chart = new google.visualization.Sankey(element);
        chart.draw(view, options);
    }

    function tooltip(from, to, value) {
        return "<div style='background-color:red'>" + value + "</div>";
    }

});

jQuery.fn.exists = function () { return this.length > 0; }

Highcharts.SparkLine = function (elem, b, c) {

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
            margin: [0, 0, 0, 0],
        },

        tooltip: {
            enabled: false
        },
        legend:{
            enabled: false
        },
        series: [],

    };

    return DefaultChart(elem, b, c, defaultOptions);
};

Highcharts.Combo = function (elem, b, c) {

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],
    defaultOptions = {

        chart: {
            renderTo: (options.chart && options.chart.renderTo) || this,
        },
        series: [],

    };

    return DefaultChart(elem, b, c, defaultOptions);
};


function DefaultChart (elem, b, c, defaultOptions) {

    var hasRenderToArg = typeof elem === 'string' || elem.nodeName,
    options = arguments[hasRenderToArg ? 1 : 0],


    options = Highcharts.merge(defaultOptions, options);

    var seriesData = $(elem).data("series");

    var counter = 0;
    $.each(seriesData, function (index, value) {
        var def = options.series[counter++];
        $.extend(true, value, def);
    });


    options.series = seriesData;

    var chart = hasRenderToArg ?
        new Highcharts.Chart(elem, options, c) :
        new Highcharts.Chart(options, b);

    //chart.series[0].data[40].select();

    var pointsToSelect = [];
    $.each(chart.series, function (index, s) {
        var focus = s.options.focus;

        if (focus === "last") {
            var points = s.points;
            var point = points[points.length - 1];
            pointsToSelect.push(point);
        }
    });


    if (pointsToSelect.length > 0) {
        chart.tooltip.refresh(pointsToSelect);
    }
    chart.reflow();

    return chart;
};

//Highcharts.SparkLine = Highcharts.Combo;