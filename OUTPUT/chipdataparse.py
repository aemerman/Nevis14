# -*- coding: utf-8 -*-
"""
Created on Fri Apr 17 10:45:57 2015

@author: Max Beck
"""


import xlsxwriter
import pprint

def chiptest_consolidate(qafile = open('QAparams_all.txt','r')):
    
    chipdata_dict = {}
    channel = 0
    for qaline in qafile.readlines():
        try:
            if qaline[0] == '*':
                linesplit = qaline.split(", ")
                chipID = int(linesplit[0][2:])
                timestamp = linesplit[1]
                run = int(linesplit[2])
                
                chipdata_dict.setdefault(chipID,{})
                chipdata_dict[chipID].setdefault(run,{})
                chipdata_dict[chipID][run]["Timestamp"] = timestamp
                
            elif "NaN" in qaline:
                linesplit = qaline.split(", ")
                channel = int(linesplit[0])
                chipdata_dict[chipID][run].setdefault(channel,False)
                
            else:
                linesplit = qaline.split(", ")
                channel = int(linesplit[0])
                calib = []
                for i in range(1,5):
                    calib.append(linesplit[i])
                drange = int(linesplit[5])
                enob =  float(linesplit[6])
                sfdr =  float(linesplit[7])
                sinad = float(linesplit[8])
                snr =   float(linesplit[9])
                freq = []
                for k in range(0,5):
                    freq.append([float(linesplit[10+2*k]),float(linesplit[11+2*k])])
                
                channeldata = chipdata_dict[chipID][run].setdefault(channel,{})
                channeldata["Range"] = drange
                channeldata["ENOB"] = enob
                channeldata["SFDR"] = sfdr
                channeldata["SINAD"] = sinad
                channeldata["SNR"] = snr
                channeldata["Calib"] = calib
                channeldata["freq"] = freq
        except:
            print qaline
    qafile.close()
    #pprint.pprint(chipdata_dict)
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
    rootfile = open('QAparams_rootformat.txt', 'wb')
    bestchips = {}
    for chipid in chip_dict:
        totalenob = 0
        lastrun = max(chip_dict[chipid].keys())
        updateparamfile.write(str.format("* {0}, {1}, {2}\r\n", chipid, chip_dict[chipid][lastrun]["Timestamp"], lastrun))
        for channel in chip_dict[chipid][lastrun]:
            channeldata = chip_dict[chipid][lastrun][channel]
            if type(channeldata) != dict:
                continue
            
            totalenob += chip_dict[chipid][lastrun][channel]["ENOB"]
            
            updateparamfile.write(str.format("{0}, ", channel))
            
            for k in range(0,4):
                updateparamfile.write(channeldata["Calib"][k])
                updateparamfile.write(", ")
                
            rootfile.write(str.format("{4} {0} {1} {2} {3} \r\n",
                                      channeldata["ENOB"], 
                                      channeldata["SFDR"], 
                                      channeldata["SINAD"],
                                      channeldata["SNR"],
                                      channeldata["Range"]))
            updateparamfile.write(str.format("{4}, {0:.4f}, {1:.4f}, {2:.4f}, {3:.4f}", 
                                  channeldata["ENOB"], 
                                  channeldata["SFDR"], 
                                  channeldata["SINAD"],
                                  channeldata["SNR"],
                                  channeldata["Range"]))
            for k in range(0,5):
                updateparamfile.write(str.format(",   {0},   {1}",
                                      channeldata["freq"][k][0],
                                      channeldata["freq"][k][1]))
            updateparamfile.write("\r\n")
        if len(bestchips.keys()) < 10:        
            bestchips[chipid] = totalenob
        else:
            cutoffenob = min(bestchips.values())
            if totalenob > cutoffenob:
                bestchips[chipid] = totalenob
                for chip in bestchips:
                    if bestchips[chip] == cutoffenob:
                        del bestchips[chip]
                        break
                    
    updateparamfile.close()
    #print bestchips
 
    return

def chiptest_writetoxlsx(chip_dict):
    
    firstchip = min(chip_dict.keys())
    chipsbygrade = []
    for i in range(0,5):
        chipsbygrade.append([])
    
    workbook = xlsxwriter.Workbook("QAparamTable.xlsx")
    worksheet = workbook.add_worksheet()
    
    gradeletters = ["A", "B", "C", "D", "F"]
    colors = ["#003399", "#33CCFF", "#99CC00", "#FF9900", "#FF0000"]
    gradeformats = []
    for i in range(0,5):
        gradeformats.append(workbook.add_format())
        gradeformats[i].set_font_color(colors[i])
    
    defectiveformat = workbook.add_format()
    defectiveformat.set_bg_color('#f48e8e')
        
    #Header
    worksheet.write(3,0,"Chip #")
    worksheet.write(3,1,"Date/Time")
    worksheet.write(3,2, "Channel")
    for i in range(0,4):
        worksheet.write(3,3+i, str.format("Cal {}",i+1))
    worksheet.write(3,7, "Dynamic Range")
    worksheet.write(3,8, "ENOB")
    worksheet.write(3,9, "SFDR")
    worksheet.write(3,10, "SINAD")
    worksheet.write(3,11, "SNR")
    for i in range(0,5):
        worksheet.write(3,12+i, str.format("freq {}", i + 1))
        worksheet.write(3,13+i, str.format("spur {}", i + 1))
    
    worksheet.write(2, 19, "Letter Grade")
    worksheet.write(2, 20, "Range")
    worksheet.write(2, 21, "ENOB")
    worksheet.write(3, 19, "A", gradeformats[0])
    worksheet.write(4, 19, "B", gradeformats[1])
    worksheet.write(5, 19, "C", gradeformats[2])
    worksheet.write(6, 19, "D", gradeformats[3])
    worksheet.write(7, 19, "F", gradeformats[4])

    worksheet.write(3, 20, ">3700")
    worksheet.write(4, 20, "3600-3699")
    #worksheet.write(5, 20, "3600-3649")
    worksheet.write(6, 20, "3500-3599")
    worksheet.write(7, 20, "<3600")
    
    worksheet.write(3, 21, ">9.9")
    worksheet.write(4, 21, "9.8-9.9")
    worksheet.write(5, 21, "9.5-9.8")
    worksheet.write(6, 21, "9.0-9.8")
    worksheet.write(7, 21, "<9.0")



    
    for chipid in chip_dict:
        
        chipgrade = 0   # 0 = A, 1 = B, etc., chip is A by default
        
        chiprow = 6 + (chipid - firstchip)*5
        row = chiprow
        lastrun = max(chip_dict[chipid].keys())
        worksheet.write(chiprow, 1, chip_dict[chipid][lastrun]["Timestamp"])
        for channel in chip_dict[chipid][lastrun]:
            channeldata = chip_dict[chipid][lastrun][channel]
            if type(channeldata) != dict:
                continue
            row = chiprow + channel - 1
            
            
            worksheet.write(row, 2, channel)
            for j in range(0,4):
                worksheet.write(row, 3+j, channeldata["Calib"][j])
                
            channelgrade = 0
            dynamicrange = channeldata["Range"]
            if dynamicrange > 4096 or dynamicrange == "Calibration Failed" or dynamicrange < 3500:
                channelgrade = 4
                worksheet.write(chiprow + 2, 0, "Calibration Failed", defectiveformat)
            elif dynamicrange <  3600:
                channelgrade = 3
            #elif dynamicrange < 3650:
            #    channelgrade = 2
            elif dynamicrange < 3700:
                channelgrade = 1

            chipgrade = max(channelgrade, chipgrade)
            worksheet.write(row, 7, channeldata["Range"], gradeformats[channelgrade])
                
            channelgrade = 0
            enob = channeldata["ENOB"]
            if enob < 9.0:
                channelgrade = 4
            elif enob < 9.5:
                channelgrade = 3
            elif enob < 9.8:
                channelgrade = 2
            elif enob < 9.9:
                channelgrade = 1
            chipgrade = max(channelgrade, chipgrade)
            worksheet.write(row, 8, channeldata["ENOB"], gradeformats[channelgrade])
            
            worksheet.write(row,9, channeldata["SFDR"])
            worksheet.write(row,10, channeldata["SINAD"])
            worksheet.write(row,11, channeldata["SNR"])
            for j in range(0,5):
                worksheet.write(row,12+j, channeldata["freq"][j][0])
                worksheet.write(row,13+j, channeldata["freq"][j][1])

        worksheet.write(chiprow + 1, 0, gradeletters[chipgrade], gradeformats[chipgrade])
        worksheet.write(chiprow, 0, chipid, gradeformats[chipgrade])
        chipsbygrade[chipgrade].append(chipid)

    
    chipdefects = open("defects.txt")
    
    for line in chipdefects.readlines():
        linesplit = line.split(' - ')
        if len(linesplit) > 1:
            chipid = int(linesplit[0])
            if chipid not in chipsbygrade[4]:
                chipsbygrade[4].append(chipid)
            row = 6 + (chipid - firstchip)*5
            worksheet.write(row, 0, chipid, defectiveformat)
            worksheet.write(row + 1, 0, linesplit[1], defectiveformat)
            worksheet.write(row + 1, 1, "Power Draw:")
            worksheet.write(row + 2, 0, linesplit[3], defectiveformat)
            worksheet.write(row + 2, 1, linesplit[2])
    
    workbook.close()
    
    for i in range(0,5):
        print str.format("{0} chips with grade {1}", len(chipsbygrade[i]), i)
        
    return chipsbygrade
                
if __name__ == '__main__':
    chipdict = chiptest_consolidate()
    chiptest_mostrecent(chipdict) 
    chiptest_writetoxlsx(chipdict)

    
