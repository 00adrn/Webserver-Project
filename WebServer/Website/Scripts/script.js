const testButton = document.getElementById("test-button");

testButton.addEventListener('click', function() {
    console.log("button pressed")
    document.body.classList.toggle("dark-mode")
});