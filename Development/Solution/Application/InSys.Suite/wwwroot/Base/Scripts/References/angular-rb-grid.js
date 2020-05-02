angular.module('rbGrid', [])
    .directive('repeatDone', function () {
        return {
            link: function ($s, $elem, attrs) {
                if ($s.$last) {
                    switch (attrs.repeatDone) {
                        case 'setPinnedFillerWidth':
                            setTimeout(function () {
                                $elem.parent().parent().parent().parent().parent().parent().find('.rbGrid-pinned-filler').width($elem.parent().parent().parent().parent().parent().parent().find('.rbGrid-pinned-container').width());
                            }, 100);
                            break;
                        case 'setStickyFillerWidth':
                            $elem.parent().parent().parent().find('.rbGrid-sticky-filler').width($elem.parent().parent().parent().find('.rbGrid-pinned-container').width());
                            break;
                    }
                }
            }
        }
    })
    .directive('setStickyPinnedMinWidth', [function () {
        return {
            link: function ($s, $elem, attrs) {
                var addedColumn = ($s.tblOptions.HasMultiSelect == true || $s.tblOptions.HasDelete == true || $s.tblOptions.HasOpenIcon == true ? 1 : 0);
                var hasPinnedColumn = ($s.tblOptions.PinnedColumns.length > 0 ? true : false);
                $(window).resize(function () {
                    $elem.parent().parent().parent().find('.rbGrid-pinned-container').find('.rbGrid-column').each(function (idx, header) {
                        idx = idx - addedColumn;
                        if (parseInt(attrs.setStickyPinnedMinWidth) == idx) {
                            var w = $(header).width();
                            $elem.width(w);
                        }
                    });
                })
                $elem.parent().parent().parent().find('.rbGrid-pinned-container').find('.rbGrid-column').each(function (idx, header) {
                    idx = idx - addedColumn;
                    if (parseInt(attrs.setStickyPinnedMinWidth) == idx) {
                        var w = $(header).width();
                        $elem.css('min-width', w);
                    }
                });
            }
        }
    }])
    .directive('setStickyMinWidth', [function () {
        return {
            link: function ($s, $elem, attrs) {
                var addedColumn = ($s.tblOptions.HasMultiSelect == true || $s.tblOptions.HasDelete == true || $s.tblOptions.HasOpenIcon == true ? 1 : 0);
                var hasPinnedColumn = ($s.tblOptions.PinnedColumns.length > 0 ? true : false);
                $(window).resize(function () {
                    $elem.parent().parent().parent().find('.rbGrid-column-container').find('.rbGrid-column').each(function (idx, header) {
                        if (!hasPinnedColumn)
                            idx = idx - addedColumn;
                        if (parseInt(attrs.setStickyMinWidth) == idx) {
                            var w = $(header).width();
                            $elem.width(w);
                        }
                    });
                })
                $elem.parent().parent().parent().find('.rbGrid-column-container').find('.rbGrid-column').each(function (idx, header) {
                    if (!hasPinnedColumn)
                        idx = idx - addedColumn;
                    if (parseInt(attrs.setStickyMinWidth) == idx) {
                        var w = $(header).width();
                        $elem.css('min-width', w);
                    }
                });
            }
        }
    }])
    .directive('rbGridResize', [function () {
        return {
            link: function ($s, $elem, $attrs) {
                $elem.on('mousedown', function (ev) {
                    var idx = parseInt($attrs.colIdx);
                    var addedColumn = ($s.tblOptions.HasMultiSelect == true || $s.tblOptions.HasDelete == true || $s.tblOptions.HasOpenIcon == true ? 1 : 0);
                    var hasPinnedColumn = ($s.tblOptions.PinnedColumns.length > 0 ? true : false);
                    var column = null;
                    var stickyColumn = null;
                    if (parseInt($attrs.rbGridResize) == 1) {
                        idx = idx + addedColumn;
                        column = $($elem.parent().parent().parent().parent().parent().find('.rbGrid-pinned-container').children()[idx]);
                        stickyColumn = $elem.parent().parent();
                    } else if (parseInt($attrs.rbGridResize) == 2) {
                        if (!hasPinnedColumn)
                            idx = idx + addedColumn;
                        idx += $s.tblOptions.PinnedColumns.length > 0 ? 1 : 0; //para sa mcount ung filler
                        column = $($elem.parent().parent().parent().parent().parent().find('.rbGrid-column-container').children()[idx + (($s.tblOptions.PinnedColumns.length == 0 ? 0 : $s.tblOptions.PinnedColumns.length - 1))]);
                        stickyColumn = $elem.parent().parent();
                        console.log(column, $elem.parent().parent())
                    } else {
                        column = $elem.parent().parent();
                    }
                    if (column.attr('origwidth') == undefined) {
                        column.attr('origwidth', column.width());
                        column.css('minWidth', column.width());

                        if (stickyColumn != null) {
                            stickyColumn.attr('origwidth', parseFloat(column.attr('origwidth')));
                            stickyColumn.css('minWidth', parseFloat(column.attr('origwidth')));
                        }

                    } else {
                        column.css('minWidth', parseFloat(column.attr('origwidth')));
                        if (stickyColumn != null) {
                            stickyColumn.css('minWidth', parseFloat(column.attr('origwidth')));
                            stickyColumn.width(column.width());
                        }
                    }

                    var pressed = false;
                    var startX, startWidth;
                    var startX2, startWidth2;

                    pressed = true;
                    startX = ev.pageX;
                    startWidth = column.width();

                    if (stickyColumn != null) {
                        startX2 = ev.pageX;
                        startWidth2 = stickyColumn.width();
                    }

                    $(document).mousemove(function (e) {
                        if (pressed) {
                            if ((startWidth + (e.pageX - startX)) < parseFloat(column.attr('origwidth')))
                                column.width(parseFloat(column.attr('origwidth')))
                            else {
                                column.width(startWidth + (e.pageX - startX) + 1);
                            }
                            column.addClass('rbGrid-resize-border');

                            if (stickyColumn != null) {
                                stickyColumn.width(startWidth2 + (e.pageX - startX2));
                                stickyColumn.addClass('rbGrid-resize-border');
                            }

                            var root = $elem.parent().parent().parent().parent().parent();
                            root.find('.rbGrid-pinned-filler').width(root.find('.rbGrid-pinned-container').width());
                            root.find('.rbGrid-sticky-filler').width(root.find('.rbGrid-sticky-pinned').width());
                        }
                    });

                    $(document).mouseup(function () {
                        if (pressed) {
                            pressed = false;
                            column.removeClass('rbGrid-resize-border');

                            if (stickyColumn != null) stickyColumn.removeClass('rbGrid-resize-border');
                        }
                    });
                })
            }
        }
    }]);