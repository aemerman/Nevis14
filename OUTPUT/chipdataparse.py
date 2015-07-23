# -*- coding: utf-8 -*-
"""
Created on Fri Apr 17 10:45:57 2015

@author: Max Beck
"""

import pprint
    
def chiptest_consolidate(qafile = open('QAparams_all.txt','r')):
    
    chipdata_dict = {}
    channel = 0
    for qaline in qafile.readlines():
        try:
            if qaline[0] == '*':
                linesplit = qaline.split(", ")
                chipID = int(linesplit[0][2:])
                date = linesplit[1]
                run = int(linesplit[2])
                
                chipdata_dict.setdefault(chipID,{})
                chipdata_dict[chipID].setdefault(run,{})
                chipdata_dict[chipID][run]["Timestamp"] = date
                channel = 1;
                
            else:
                linesplit = qaline.split(", ")
                channel = int(linesplit[0])
                calib = []
                for i in range(1,5):
                    calib.append(linesplit[i])
                enob =  float(linesplit[5])
                sfdr =  float(linesplit[6])
                sinad = float(linesplit[7])
                snr =   float(linesplit[8])
                freq = []
                for k in range(0,5):
                    freq.append([float(linesplit[9+2*k]),float(linesplit[10+2*k])])
                
                chipdata_dict[chipID][run].setdefault(channel,{})
                chipdata_dict[chipID][run][channel]["ENOB"] = enob
                chipdata_dict[chipID][run][channel]["SFDR"] = sfdr
                chipdata_dict[chipID][run][channel]["SINAD"] = sinad
                chipdata_dict[chipID][run][channel]["SNR"] = snr
                chipdata_dict[chipID][run][channel]["Calib"] = calib
                chipdata_dict[chipID][run][channel]["freq"] = freq
                channel +=1
        except:
            print("EndofFile")
    qafile.close()
    pprint.pprint(chipdata_dict)
    return chipdata_dict
    
    
def chiptest_optimize(chip_dict):
    #pprint.pprint(chip_dict)
    for chipid in chip_dict:
        enobmax = [-100 for x in range(4)]
        maxrun = [-1 for x in range(4)]
        for run in chip_dict[chipid]:
            for channel in chip_dict[chipid][run]:
                if type(channel) == int:
                    enob = chip_dict[chipid][run][channel]['ENOB']
                    enobmax[channel-1] = max([enob, enobmax[channel-1]])
                    if enobmax[channel-1] == enob:
                        maxrun[channel-1] = run
    
    for chipid in chip_dict:
        print "Chip " + str(chipid)
        for k in range(0,4):
            print "Channel " + str(k+1) +": " + str(enobmax[k]) + " (Run " + str(maxrun[k]) + ")"
            
    return
    
def chiptest_mostrecent(chip_dict):
    updateparamfile = open('QAparams_clean.txt', 'wb')
    missingchannels = False
    for chipid in chip_dict:
        lastrun = max(chip_dict[chipid].keys())
        updateparamfile.write(str.format("* {0}, {1}, {2}\r\n", chipid, chip_dict[chipid][lastrun]["Timestamp"], lastrun))
        for channel in range(1,5):
            if channel in chip_dict[chipid][lastrun]:
                updateparamfile.write(str.format("{0}, ", channel))
                for k in range(0,4):
                    updateparamfile.write(chip_dict[chipid][lastrun][channel]["Calib"][k])
                    updateparamfile.write(", ")
                updateparamfile.write(str.format("{0:.4f}, {1:.4f}, {2:.4f}, {3:.4f}", 
                                      chip_dict[chipid][lastrun][channel]["ENOB"], 
                                      chip_dict[chipid][lastrun][channel]["SFDR"], 
                                      chip_dict[chipid][lastrun][channel]["SINAD"],
                                      chip_dict[chipid][lastrun][channel]["SNR"]))
                for k in range(0,5):
                    updateparamfile.write(str.format(",   {0},   {1}",
                                          chip_dict[chipid][lastrun][channel]["freq"][k][0],
                                          chip_dict[chipid][lastrun][channel]["freq"][k][1]))
                updateparamfile.write("\r\n")
            else:
                missingchannels = True
    if(missingchannels):
        print ("Warning! Newly created file may be missing channels")
    updateparamfile.close()
    return
   
if __name__ == '__main__':
    chiptest_mostrecent(chiptest_consolidate())                                