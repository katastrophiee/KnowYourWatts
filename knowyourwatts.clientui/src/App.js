import React, { useEffect } from 'react';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';

function App() {
   useEffect(() => {
        function displayTime() {
            const now = new Date();
            const hours = String(now.getHours()).padStart(2, '0');
            const minutes = String(now.getMinutes()).padStart(2, '0');
            document.getElementById('time').textContent = hours + ':' + minutes;
        }
        displayTime();
        
        const intervalId = setInterval(displayTime, 60000);
        
        return () => clearInterval(intervalId);
    }, []); 
    return (
        <div className="App">
            <header className="App-header">
                <div id="time"></div>
            </header>
            <div>
            <nav>
                <ul class="nav nav-tabs" id="myTab" role="tablist">
                    <li class="nav-item" role="presentation">
                        <button class="nav-link active" id="current-tab" data-bs-toggle="tab" data-bs-target="#current-tab-pane" type="button" role="tab" aria-controls="current-tab-pane" aria-selected="true">Current Usages</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="todays-tab" data-bs-toggle="tab" data-bs-target="#todays-tab-pane" type="button" role="tab" aria-controls="todays-tab-pane" aria-selected="false">Today's Usage</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="weekly-tab" data-bs-toggle="tab" data-bs-target="#weekly-tab-pane" type="button" role="tab" aria-controls="weekly-tab-pane" aria-selected="false">Weekly Usage</button>
                    </li>
                </ul>
            </nav>
                <div class="tab-content" id="myTabContent">
                    <div class="tab-pane fade show active" id="current-tab-pane" role="tabpanel" aria-labelledby="current-tab" tabindex="0">.1.</div>
                    <div class="tab-pane fade" id="todays-tab-pane" role="tabpanel" aria-labelledby="todays-tab" tabindex="0">.2.</div>
                    <div class="tab-pane fade" id="weekly-tab-pane" role="tabpanel" aria-labelledby="weekly-tab" tabindex="0">.3.</div>
                </div>
            </div>


        </div>
          
    );
}

export default App;

