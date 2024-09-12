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
    var el = document.getElementById(elementId);
    if (el) {
        el.value = newValue;
    }
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
    console.log(element);
    if (element) {
        element.style.display = 'block';
    }
}

window.hideElementById = (id) => {
    var element = document.getElementById(id);
    console.log(element);
    if (element) {
        element.style.display = 'none';
    }
}

var _isStartCellInFocus = true;
window.StartCellClicked = (isStartCellClicked, tableID) => {
    _isStartCellInFocus = isStartCellClicked;

    // mark startCell or endCell control
    var startCellID = `${tableID}-start-cell`;
    var endCellID = `${tableID}-end-cell`;
    var startCellElement = document.getElementById(startCellID);
    var endCellElement = document.getElementById(endCellID);

    if (isStartCellClicked) {
        if (endCellElement.classList.contains('marked-border')) {
            endCellElement.classList.remove('marked-border');
        }
        if (!startCellElement.classList.contains('marked-border')) {
            startCellElement.classList.add('marked-border');
        }
    }
    else {
        if (startCellElement.classList.contains('marked-border')) {
            startCellElement.classList.remove('marked-border');
        }
        if (!endCellElement.classList.contains('marked-border')) {
            endCellElement.classList.add('marked-border');
        }
    }
}

window.CellClick = (employeeJson, col, tableID, totalRows, totalCols) => {
    var employee = JSON.parse(employeeJson);
    if (employee.IsEditMode) {
        return;
    }

    var cellIdentifier = `R${employee.RowID}-C${col}`;
    var startCellID = `${tableID}-start-cell`;
    var endCellID = `${tableID}-end-cell`;

    console.log("cellIdentifier", cellIdentifier);
    console.log("startCellID", startCellID);
    console.log("endCellID", endCellID);

    var startCell = document.getElementById(startCellID);
    var endCell = document.getElementById(endCellID);
    var startCellValue = startCell.value;
    var endCellValue = endCell.value;

    console.log("startCell", startCell);
    console.log("endCell", endCell);
    console.log("startCellValue", startCellValue);
    console.log("endCellValue", endCellValue);

    var areBothFilled = startCellValue && endCellValue;
    var areBothEmpty = !startCellValue && !endCellValue;

    console.log("areBothFilled", areBothFilled);
    console.log("areBothEmpty", areBothEmpty);

    // Check if the element is currently in focus
    
    console.log("_isStartCellInFocus", _isStartCellInFocus); // Output: true or false

    var startRow = 0;
    var endRow = 0;
    var startCol = 0;
    var endCol = 0;
    var isRowReversed = false;
    var isColReversed = false;
    // Save values first
    var savedStartRow = startRow;
    var savedEndRow = endRow;
    var savedStartCol = startCol;
    var savedEndCol = endCol;

    // when you click a cell in the table grid, determine if both start and end cell controls are empty
    if (areBothEmpty) {
        startCell.value = cellIdentifier;
        startCellValue = cellIdentifier;
        
        StartCellClicked(true, tableID);
        
        // get existing startCell value first
        var startCellSplit = startCellValue.split('-');

        // mark selected table cell as start cell
        startRow = parseInt(startCellSplit[0].replace('R', ''));
        startCol = parseInt(startCellSplit[1].replace('C', ''));
        endRow = parseInt(startRow);
        endCol = parseInt(startCol);
    }
    else {
        if (areBothFilled) {
            // Determine first were to put te cellIdentifier value.
            // get existing values first
            var startCellSplit = startCellValue.split('-');
            startRow = parseInt(startCellSplit[0].replace('R', ''));
            startCol = parseInt(startCellSplit[1].replace('C', ''));

            var endCellSplit = endCellValue.split('-');
            endRow = parseInt(endCellSplit[0].replace('R', ''));
            endCol = parseInt(endCellSplit[1].replace('C', ''));

            // Save values first
            savedStartRow = startRow;
            savedEndRow = endRow;
            savedStartCol = startCol;
            savedEndCol = endCol;

            //================================================================>
            // Case 1: Click cell in between currently selected cell:
            // Currently selected cell range: [2,2,4,4] [row,col,row,col]
            // Currently clicked cell = 3,3
            // Should depend on _isStartCellInFocus
            // if _isStartCellInFocus = true, store to startCell.value
            // else store to endCell.value

            // Check if values are in between range: 
            // Note: Remember to use saved variables in comparing values
            var isRowInBetweenRange = employee.RowID >= savedStartRow && employee.RowID <= savedEndRow;
            var isColInBetweenRange = col >= savedStartCol && col <= savedEndCol;
            var isBothInRange = isRowInBetweenRange && isColInBetweenRange;
            if (isBothInRange) {
                if (_isStartCellInFocus) {
                    //store to startCell.value
                    startRow = employee.RowID;
                    startCol = col;
                    startCell.value = cellIdentifier;
                }
                else {
                    endRow = employee.RowID;
                    endCol = col;
                    endCell.value = cellIdentifier;
                }
            }

            //================================================================>
            // Case 2: Click cell above the row of currently selected cell:
            // Currently selected cell range: [2,2,4,4] [row,col,row,col]
            // Currently clicked cell = 1,3 or any row above startCell.value row
            //  regardless of the clicked column
            // This should immediately set _isStartCellInFocus = true
            // store to startCell.value
            var isRowAboveRange = employee.RowID <= savedStartRow;
            if (isRowAboveRange) {
                StartCellClicked(true, tableID);
                startCell.value = cellIdentifier;
                startRow = employee.RowID;
                startCol = col;
            }

            //================================================================>
            // Case 3: Click cell below the row of currently selected cell:
            // Currently selected cell range: [2,2,4,4] [row,col,row,col]
            // Currently clicked cell = 4,3 or any row below endCell.value row
            //  regardless of the clicked column
            // This should immediately set _isStartCellInFocus = false
            // store to endCell.value
            var isRowBelowRange = employee.RowID >= savedEndRow;
            if (isRowBelowRange) {
                StartCellClicked(false, tableID);
                endCell.value = cellIdentifier;
                endRow = employee.RowID;
                endCol = col;
            }

            //================================================================>
            // Case 4: Click cell on the same row of selected range but
            //  above the col of currently selected cell:
            // Currently selected cell range: [2,2,4,4] [row,col,row,col]
            // Currently clicked cell = 2,1 or any col above startCell.value col
            //  on the any row in the currently selected range
            // This should immediately set _isStartCellInFocus = true
            // store to startCell.value

            var isColAboveRange = col <= savedStartCol;
            if (isColAboveRange && isRowInBetweenRange) {
                StartCellClicked(true, tableID);
                startCell.value = cellIdentifier;
                startRow = employee.RowID;
                startCol = col;
            }

            //================================================================>
            // Case 5: Click cell on the same row of selected range but
            //  below the col of currently selected cell:
            // Currently selected cell range: [2,2,4,4] [row,col,row,col]
            // Currently clicked cell = 2,5 or any col below endCell.value col
            //  on the any row in the currently selected range
            // This should immediately set _isStartCellInFocus = false
            // store to endCell.value

            var isColBelowRange = col >= savedEndCol;
            if (isColBelowRange && isRowInBetweenRange) {
                StartCellClicked(false, tableID);
                endCell.value = cellIdentifier;
                endRow = employee.RowID;
                endCol = col;
            }
        }
        else {
            // check if startCellValue has value
            if (startCellValue) {
                // get existing values first
                var startCellSplit = startCellValue.split('-');
                startRow = parseInt(startCellSplit[0].replace('R', ''));
                startCol = parseInt(startCellSplit[1].replace('C', ''));

                endRow = employee.RowID;
                endCol = col;

                StartCellClicked(false, tableID);
                endCell.value = cellIdentifier;
            }
            else {
                // get existing values first
                var endCellSplit = endCellValue.split('-');
                endRow = parseInt(endCellSplit[0].replace('R', ''));
                endCol = parseInt(endCellSplit[1].replace('C', ''));

                startRow = employee.RowID;
                startCol = col;

                StartCellClicked(true, tableID);
                startCell.value = cellIdentifier;
            }

            // Save values first
            savedStartRow = startRow;
            savedEndRow = endRow;
            savedStartCol = startCol;
            savedEndCol = endCol;

            // Check if values should be reversed
            isRowReversed = startRow > endRow;
            isColReversed = startCol > endCol;

            startRow = isRowReversed ? savedEndRow : startRow;
            endRow = isRowReversed ? savedStartRow : endRow;
            startCol = isColReversed ? savedEndCol : startCol;
            endCol = isColReversed ? savedStartCol : endCol;

        }
    }

    // and then mark it.
    var shouldMark = true;
    
    //var savedStartRow = startRow;
    //var savedEndRow = endRow;
    //var savedStartCol = startCol;
    //var savedEndCol = endCol;
    //startRow = isRowReversed ? savedEndRow : startRow;
    //endRow = isRowReversed ? savedStartRow : endRow;
    //startCol = isColReversed ? savedEndCol : startCol;
    //endCol = isColReversed ? savedStartCol : endCol;

    //startCellValue = `R${startRow}-C${startCol}`;;
    //endCellValue = `R${endRow}-C${endCol}`;;

    //setValue(startCellID, startCellValue);
    //setValue(endCellID, endCellValue);

    toggleCellBorders(startRow, endRow, startCol, endCol, totalRows, totalCols, tableID, shouldMark);

}

// window.CellClick = (employee, col, tableID, totalRows, totalCols) => {
window.CellClick2 = (employeeJson) => {
    alert(employeeJson);
}

window.addClassToElement = (elementId, className) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.classList.add(className);
    }
};

window.removeClassFromElement = (elementId, className) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.classList.remove(className);
    }
};

