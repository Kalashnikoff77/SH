// Затухание и появление числа (напр.: изменилось кол-во непрочитанных сообщений в чате)
function ChangeNumberFadeInOut(tagClass, number, isShowZero) {
    var tag = $('.' + tagClass);
    if (tag.text() != number) {
        tag.fadeOut(200, function () {
            tag.text('');
            if (number > 0 || isShowZero) {
                tag.text(number);
                tag.fadeIn(200);
            }
        })
    }
}

// Увеличить число в теге
function IncreaseNumberFadeInOut(tagClass) {
    $('.' + tagClass)
        .each(function () {
            $(this).fadeOut(200, function () {
                var currentNumber = Number($(this).text());
                $(this).text(currentNumber + 1);
                $(this).fadeIn(200);
            });
        })
}

// Уменьшить число в теге
function DecreaseNumberFadeInOut(tagClass, isShowZero) {
    $('.' + tagClass)
        .each(function () {
            $(this).fadeOut(200, function () {
                var currentNumber = Number($(this).text());
                $(this).text('');
                currentNumber--;
                if (currentNumber > 0 || isShowZero) {
                    $(this).text(currentNumber);
                    $(this).fadeIn(200);
                }
            });
        })
}


// Прокрутка div вниз
function ScrollDivToBottom(tagId) {
    $('#' + tagId).scrollTop($('#' + tagId)[0].scrollHeight);
}

function Redirect(uri) { window.location.replace(uri) }
