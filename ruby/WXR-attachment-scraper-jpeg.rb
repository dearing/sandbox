#!/usr/bin/ruby

require 'net/http'
require 'cloudfiles'
require 'colorize'
require 'uri'

i = 0
skipto = ARGV[0].to_i
snooze = ARGV[1].to_f

cf = CloudFiles::Connection.new(:username => "username", :api_key => "api-key", :snet => true)
container = cf.container('my-container')

Net::HTTP.start("example.tld") do |http|

# Read file for urls to test; download and publish if not extant
work = File.readlines('work.txt')
	work.each do |line|
		i = i +1
		# quick and dirty 'skipto' index for sessions restarts
		if i < skipto then next end
		sleep(snooze)
		URI.extract(line, "http").each do |url|
			url =~ /(http:\/\/example.tld\/)/
			name = $'

			print "\t[#{i}/#{work.length}] check: #{url}"
			# if extant move on; else publish to CDN
			if !container.objects.include?(name)
				print "\rFAIL\n".red
				print "\t#{name} => pushing to CDN...".magenta
				resp = http.get(url)
				object = container.create_object name, false
				object.write resp.body, {'Content-Type' => 'image/jpeg'}
				print "done\n".green
			else
				print "\rPASS\n".green
			end
		end
	end
end # NET::HTTP
