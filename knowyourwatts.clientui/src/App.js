import React, { useEffect } from 'react';
import './App.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'bootstrap/dist/js/bootstrap.bundle.min.js';
import MonitorData from './MonitorData';

function App() {
    var monitorData = new MonitorData('0', '0', '0');
    var data = monitorData.getData();
    window.onload = function() {
        document.getElementById('current-tab-pane').textContent = data.totalBill;
        document.getElementById('todays-tab-pane').textContent = data.currentRateMoney;
        document.getElementById('weekly-tab-pane').textContent = data.currentRateKwh;
    }
        
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
            <div class="p-2">
                <ul class="nav nav-tabs nav-justified py-1" id="myTab" role="tablist">
                    <li class="nav-item pe-1" role="presentation">
                        <button class="nav-link active" id="current-tab" data-bs-toggle="tab" data-bs-target="#current-tab-pane" type="button" role="tab" aria-controls="current-tab-pane" aria-selected="true">Current Usages</button>
                    </li>
                    <li class="nav-item pe-1" role="presentation">
                        <button class="nav-link" id="todays-tab" data-bs-toggle="tab" data-bs-target="#todays-tab-pane" type="button" role="tab" aria-controls="todays-tab-pane" aria-selected="false">Today's Usage</button>
                    </li>
                    <li class="nav-item" role="presentation">
                        <button class="nav-link" id="weekly-tab" data-bs-toggle="tab" data-bs-target="#weekly-tab-pane" type="button" role="tab" aria-controls="weekly-tab-pane" aria-selected="false">Weekly Usage</button>
                    </li>
                </ul>
                <div class="tab-content" id="myTabContent">
                    <div class="tab-pane fade show active py-4" id="current-tab-pane" role="tabpanel" aria-labelledby="current-tab" tabindex="0">
                        <div class="container row">
                            <p class="col-10 align-top fs-4 fw-bold px-4">Usage:</p>
                            <div class="col-2 text-center"><svg class="icon"xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path fill="#FFD43B" d="M0 256L28.5 28c2-16 15.6-28 31.8-28H228.9c15 0 27.1 12.1 27.1 27.1c0 3.2-.6 6.5-1.7 9.5L208 160H347.3c20.2 0 36.7 16.4 36.7 36.7c0 7.4-2.2 14.6-6.4 20.7l-192.2 281c-5.9 8.6-15.6 13.7-25.9 13.7h-2.9c-15.7 0-28.5-12.8-28.5-28.5c0-2.3 .3-4.6 .9-6.9L176 288H32c-17.7 0-32-14.3-32-32z" /></svg></div>
                        </div>
                        <p class="px-3">&pound;1.80</p>
                        <p class="px-3">18KW</p>
                    </div>
                    <div class="tab-pane fade py-4" id="todays-tab-pane" role="tabpanel" aria-labelledby="todays-tab" tabindex="0">
                        <div class="container row">
                            <p class="col-10 align-top fs-4 fw-bold px-4">Usage:</p>
                            <div class="col-2 text-center"><svg class="icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path fill="#FFD43B" d="M0 256L28.5 28c2-16 15.6-28 31.8-28H228.9c15 0 27.1 12.1 27.1 27.1c0 3.2-.6 6.5-1.7 9.5L208 160H347.3c20.2 0 36.7 16.4 36.7 36.7c0 7.4-2.2 14.6-6.4 20.7l-192.2 281c-5.9 8.6-15.6 13.7-25.9 13.7h-2.9c-15.7 0-28.5-12.8-28.5-28.5c0-2.3 .3-4.6 .9-6.9L176 288H32c-17.7 0-32-14.3-32-32z" /></svg></div>
                        </div>
                        <p class="px-3">&pound;1.80</p>
                        <p class="px-3">18KW</p>
                    </div>
                    <div class="tab-pane fade py-4" id="weekly-tab-pane" role="tabpanel" aria-labelledby="weekly-tab" tabindex="0">
                        <div class="container row">
                            <p class="col-10 align-top fs-4 fw-bold px-4">Usage:</p>
                            <div class="col-2 text-center"><svg class="icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 384 512"><path fill="#FFD43B" d="M0 256L28.5 28c2-16 15.6-28 31.8-28H228.9c15 0 27.1 12.1 27.1 27.1c0 3.2-.6 6.5-1.7 9.5L208 160H347.3c20.2 0 36.7 16.4 36.7 36.7c0 7.4-2.2 14.6-6.4 20.7l-192.2 281c-5.9 8.6-15.6 13.7-25.9 13.7h-2.9c-15.7 0-28.5-12.8-28.5-28.5c0-2.3 .3-4.6 .9-6.9L176 288H32c-17.7 0-32-14.3-32-32z" /></svg></div>
                        </div>
                        <p class="px-3">&pound;1.80</p>
                        <p class="px-3">18KW</p>
                    </div>
                </div>
            </div>


        </div>
          
    );
}

export default App;

