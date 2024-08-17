// wwwroot/js/site.js
window.downloadFile = function (filePath) {
    fetch(filePath)
        .then(response => {
            if (!response.ok) {
                throw new Error('File not found');
            }
            return response.blob();
        })
        .then(blob => {
            // Create a URL for the blob
            var url = window.URL.createObjectURL(blob);

            // Create a link element
            var a = document.createElement("a");
            document.body.appendChild(a);
            a.style = "display: none";

            // Set the href and download attributes of the link
            a.href = url;

            // Extract the file name from the filePath
            var urlObject = new URL(filePath);
            var pathname = urlObject.pathname;
            var filename = pathname.substring(pathname.lastIndexOf('/') + 1);

            a.download = decodeURIComponent(filename); // decodeURIComponent is used to decode any encoded characters

            // Simulate a click on the link
            a.click();

            // Remove the link from the DOM
            document.body.removeChild(a);
        })
        .catch(error => console.error('Error:', error));
}

window.sendAlert = (message) => {
    alert(message);
};

window.simulateEnterKeyPress = (elementId) => {
    let element = document.getElementById(elementId);
    if (element) {
        let enterEvent = new KeyboardEvent('keydown', { bubbles: true, cancelable: true, key: 'Enter' });
        element.dispatchEvent(enterEvent);
    }
};

window.setInputValue = (elementId, value) => {
    let element = document.getElementById(elementId);
    if (element) {
        element.value = value;
        let event = new Event('input', { bubbles: true });
        element.dispatchEvent(event);
    }
};

window.getElementValue = (elementId) => {
    let element = document.getElementById(elementId);
    return element ? element.value : null;
};

window.setElementValue = (elementId, elementValue) => {
    let element = document.getElementById(elementId);
    element.value = elementValue;
};

window.setElementText = (elementId, elementValue) => {
    let element = document.getElementById(elementId);
    element.innerText = elementValue;
};

window.selectBlazoredTypeaheadItem = (elementId, elementText) => {
    let inputElement = document.getElementById(elementId);
    if (inputElement) {
        let parentElement = inputElement.closest('.blazored-typeahead');
        if (parentElement) {
            let spans = parentElement.querySelectorAll('.blazored-typeahead__results span');
            for (let i = 0; i < spans.length; i++) {
                if (spans[i].textContent === elementText) {
                    spans[i].click();
                    break;
                }
            }
        }
    }
};

window.monitorElementValue = (elementId, dotNetRef) => {
    const element = document.getElementById(elementId);

    // Define the event listener function
    const changeEventListener = function (element, dotNetRef) {
        var model = null;
        return function () {
            debugger;
            const idValue = element.value;
            console.log(idValue);

            // Alert to indicate that the change event has been triggered
            alert('Change event triggered with value: ' + idValue);

            // Call the .NET method passing the ID value and model name
            dotNetRef.invokeMethodAsync('FetchDetails', idValue)
                .then(result => {
                    // Handle the result if needed
                    console.log(result);
                    model = result;
                })
                .catch(error => {
                    // Handle any errors
                    console.error(error);
                });
        };
    };

    // Create a new Event object
    const event = new Event('change');

    // Attach the event listener to the 'change' event of the element
    element.addEventListener('change', changeEventListener(element, dotNetRef));

    // Dispatch the 'change' event
    element.dispatchEvent(event);

    // Optionally, you can return the event listener if you want to remove it later
    return model;
};

// wwwroot/custom.js

window.attachOnChange = () => {
    var hiddenInput = document.getElementById("hdn-selected-EmployeeID-value");

    if (hiddenInput) {
        hiddenInput.onchange = elementChange;
    }
};

function elementChange() {
    // Handle onchange event here
    // You can call a C# method using JavaScript interop if needed
    console.log("Value changed:", this.value);
}

window.DOMCleanup = {
    createObserver: function (elementId, dotnetHelper) {
        let tries = 0;

        const checkElement = () => {
            const targetNode = document.getElementById(elementId);

            if (targetNode) {
                this.observeElement(targetNode, dotnetHelper);
            } else if (tries < 5) {
                tries++;
                setTimeout(checkElement, 1000);
            } else {
                console.error('Element with id ' + elementId + ' does not exist after 5 seconds');
            }
        };

        checkElement();
    },

    observeElement: function (element, dotnetHelper) {
        const observer = new MutationObserver(mutationsList => {
            for (const mutation of mutationsList) {
                if (mutation.type === 'attributes' && mutation.attributeName === 'value') {
                    const newValue = element.value;
                    console.log("Value changed:", newValue);
                    dotnetHelper.invokeMethodAsync('InvokeHandleSelectedEmployeeValueChange', newValue);
                }
            }
        });

        observer.observe(element, { attributes: true });
    }
};

window.setValue = (elementId, newValue) => {
    document.getElementById(elementId).value = newValue;
};

window.getValue = (elementId) => {
    var el = document.getElementById(elementId);

    var elementValue = null
    if (el) {
        elementValue = el.value;
    }

    return elementValue;
};

window.toggleCellBorders = (startRow, endRow, startCol, endCol, totalRows, totalCols, tableId, shouldMark) => {

    // ran through all the cells 
    for (var row = 1; row < totalRows + 1; row++) {
        for (var col = 1; col < totalCols + 1; col++) {
            var elementId = tableId + "-" + row + "-" + col;
            var cellElement = document.getElementById(elementId);
            if (cellElement) {

                var isRowInBetweenRange = row >= startRow && row <= endRow;
                var isColInBetweenRange = col >= startCol && col <= endCol;
                if (isRowInBetweenRange && isColInBetweenRange) {
                    // mark if row and col are in the range
                    if (shouldMark) {
                        if (!cellElement.classList.contains('marked-border')) {
                            cellElement.classList.add('marked-border');
                        }
                    }
                    else {
                        if (cellElement.classList.contains('marked-border')) {
                            cellElement.classList.remove('marked-border');
                        }
                    }
                }
                else {
                    // unmark if row and col are not in the range
                    if (cellElement.classList.contains('marked-border')) {
                        cellElement.classList.remove('marked-border');
                    }
                }

            }
        }
    }
};

window.logToConsole = (message) => {
    console.log(message);
};

window.scrollToBottom = (divId) => {
    var div = document.getElementById(divId);
    if (div) {
        div.scrollTop = div.scrollHeight;
    }
}

window.showElementById = (id) => {
    var element = document.getElementById(id);
    alert(element);
    if (element) {
        element.style.display = 'block';
    }
}

window.hideElementById = (id) => {
    var element = document.getElementById(id);
    alert(element);
    if (element) {
        element.style.display = 'none';
    }
}
