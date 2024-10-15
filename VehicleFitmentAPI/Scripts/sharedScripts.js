
document.addEventListener('DOMContentLoaded', function () {
    fetch('/api/vehicle')
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok ' + response.statusText);
            }
            return response.json();
        })
        .then(data => {
            var dropdowns = document.getElementsByClassName('vehicle-dropdown');
            Array.from(dropdowns).forEach(dd => {
                data.forEach(vehicle => {
                    var option = document.createElement('option');
                    option.value = vehicle.VehicleId;
                    option.text = vehicle.ModelYear + " " + vehicle.Make + " " + vehicle.Model + " " + vehicle.Trim;
                    dd.add(option);
                });
            });

        })
        .catch(error => {
            console.error('Error fetching vehicles:', error);
        });
});