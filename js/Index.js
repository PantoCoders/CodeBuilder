$(function() {
    $(".SelectType").autocompleteArray([
        'bigint', 'binary', 'bit', 'char', 'date', 'datetime', 'datetime2', 'datetimeoffset',
        'decimal', 'float', 'geography', 'geometry', 'hierarchyid', 'image', 'int', 'money',
        'nchar', 'ntext', 'numeric', 'nvarchar', 'real', 'smalldatetime', 'smallint', 'smallmoney',
        'sql_variant', 'sysname', 'text', 'time', 'timestamp', 'tinyint', 'uniqueidentifier',
        'varbinary', 'varchar', 'xml'
    ], {
        delay: 10,
        minChars: 0,
        matchSubset: 1,
        width: 400, //提示的宽度
        scrollHeight: 300, //提示的高度
        // onItemSelect:selectItem,
        // onFindValue:findValue,
        autoFill: true,
        maxItemsToShow: 10
    });

});

(function($) {
    jQuery.expr[':'].Contains = function(a, i, m) {
        return (a.textContent || a.innerText || "").toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
    };

    function filterList(header, list) {
        var form = $("<form>").attr({ "class": "filterform", "action": "#" }),
            input = $("<input>").attr({ "class": "filterinput form-control", "type": "text" });
        $(form).append(input).appendTo(header);
        $(input)
            .change(function() {
                var filter = $(this).val();
                if (filter) {
                    $matches = $(list).find('span:Contains(' + filter + ')').parent();
                    $('li', list).not($matches).slideUp(50);
                    $matches.slideDown(50);
                } else {
                    $(list).find("li").slideDown(50);
                }
                return false;
            })
            .keyup(function() {
                $(this).change();
            });
    }
    $(function() {
        filterList($("#form"), $("#list"));
    });
}(jQuery));